using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository      _repo;
        private readonly IConfiguration       _configuration;
        private readonly INotificationService _notifications;
        private readonly IEmailService        _emailService;

        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes    = 15;

        private const int OtpExpiryMinutes         = 10;
        private const int MaxOtpAttempts           = 5;
        private const int ResetSessionExpiryMinutes = 10;

        public AuthService(
            IAuthRepository repo,
            IConfiguration configuration,
            INotificationService notifications,
            IEmailService emailService)
        {
            _repo          = repo;
            _configuration = configuration;
            _notifications = notifications;
            _emailService  = emailService;
        }

        public async Task<AuthLoginResult> LoginAsync(LoginRequestDto request)
        {
            var user = await _repo.GetUserWithRoleByEmailAsync(request.Email);
            if (user == null)
                return new AuthLoginResult { ErrorCode = "INVALID_CREDENTIALS" };

            if (!user.IsActive)
                return new AuthLoginResult { ErrorCode = "ACCOUNT_DISABLED" };

            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                var minutes = (int)Math.Ceiling((user.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes);
                return new AuthLoginResult { ErrorCode = "ACCOUNT_LOCKED", MinutesRemaining = minutes };
            }

            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value <= DateTime.UtcNow)
            {
                user.LockoutUntil        = null;
                user.FailedLoginAttempts = 0;
            }

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordCorrect)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutUntil        = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    user.FailedLoginAttempts = 0;
                    await _repo.SaveChangesAsync();

                    // Notify all admins about the lockout
                    var lockedName = $"{user.FirstName} {user.LastName}";
                    await _notifications.NotifyAdminsAsync(
                        type:    NotificationType.AccountLocked,
                        title:   "Account Locked Out",
                        message: $"\"{lockedName}\" ({user.Email}) was locked out after {MaxFailedAttempts} failed login attempts.");

                    return new AuthLoginResult { ErrorCode = "ACCOUNT_LOCKED", MinutesRemaining = LockoutMinutes };
                }

                await _repo.SaveChangesAsync();
                return new AuthLoginResult
                {
                    ErrorCode         = "INVALID_CREDENTIALS",
                    AttemptsRemaining = MaxFailedAttempts - user.FailedLoginAttempts
                };
            }

            user.FailedLoginAttempts = 0;
            user.LockoutUntil        = null;
            await _repo.SaveChangesAsync();

            var token = GenerateJwt(user);

            return new AuthLoginResult
            {
                Success  = true,
                Response = new LoginResponseDto
                {
                    UserId    = user.Id,
                    Token     = token,
                    Email     = user.Email,
                    FirstName = user.FirstName,
                    LastName  = user.LastName,
                    Role      = user.Role.Name
                }
            };
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _repo.GetActiveUserWithRoleByEmailAsync(email);
            if (user == null) return false;

            var existing = await _repo.GetActiveTokensForUserAsync(user.Id);
            foreach (var t in existing) t.IsUsed = true;

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            _repo.AddPasswordResetToken(new PasswordResetToken
            {
                UserId        = user.Id,
                OtpHash       = BCrypt.Net.BCrypt.HashPassword(otp),
                OtpExpiresAt  = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                OtpAttempts   = 0,
                IsOtpVerified = false,
                // Placeholder until the OTP is verified — never handed to the client
                Token         = Guid.NewGuid().ToString("N"),
                ExpiresAt     = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                IsUsed        = false,
                CreatedAt     = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Your password reset code",
                EmailService.BuildOtpEmailBody(user.FirstName, otp, OtpExpiryMinutes));

            return true;
        }

        public async Task<VerifyOtpResult> VerifyOtpAsync(string email, string otp)
        {
            var user = await _repo.GetActiveUserWithRoleByEmailAsync(email);
            if (user == null)
                return new VerifyOtpResult { Success = false, Error = "Invalid or expired code." };

            var record = await _repo.GetActiveOtpForUserAsync(user.Id);
            if (record == null)
                return new VerifyOtpResult { Success = false, Error = "Invalid or expired code. Please request a new one." };

            if (record.OtpAttempts >= MaxOtpAttempts)
            {
                record.IsUsed = true;
                await _repo.SaveChangesAsync();
                return new VerifyOtpResult { Success = false, Error = "Too many incorrect attempts. Please request a new code." };
            }

            if (!BCrypt.Net.BCrypt.Verify(otp, record.OtpHash))
            {
                record.OtpAttempts++;
                await _repo.SaveChangesAsync();
                var remaining = MaxOtpAttempts - record.OtpAttempts;
                return new VerifyOtpResult
                {
                    Success           = false,
                    Error             = "Incorrect code.",
                    AttemptsRemaining = remaining
                };
            }

            record.IsOtpVerified = true;
            record.Token         = Guid.NewGuid().ToString("N");
            record.ExpiresAt     = DateTime.UtcNow.AddMinutes(ResetSessionExpiryMinutes);
            await _repo.SaveChangesAsync();

            return new VerifyOtpResult { Success = true, ResetToken = record.Token };
        }

        public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
        {
            var record = await _repo.GetVerifiedResetTokenAsync(resetToken);
            if (record == null) return false;

            record.User.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(newPassword);
            record.User.FailedLoginAttempts = 0;
            record.User.LockoutUntil        = null;
            record.IsUsed                   = true;

            await _repo.SaveChangesAsync();
            return true;
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim(ClaimTypes.GivenName, user.FirstName)
            };

            var jwtToken = new JwtSecurityToken(
                issuer:             _configuration["Jwt:Issuer"],
                audience:           _configuration["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["Jwt:ExpiryHours"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
