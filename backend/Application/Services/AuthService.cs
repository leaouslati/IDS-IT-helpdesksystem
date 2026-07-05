using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository      _repo;
        private readonly IConfiguration       _configuration;
        private readonly INotificationService _notifications;

        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes    = 15;

        public AuthService(IAuthRepository repo, IConfiguration configuration, INotificationService notifications)
        {
            _repo          = repo;
            _configuration = configuration;
            _notifications = notifications;
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

        public async Task<string?> ForgotPasswordAsync(string email)
        {
            // We don't expose whether the email exists — always return success to the caller
            var user = await _repo.GetActiveUserWithRoleByEmailAsync(email);
            if (user == null) return null;

            var existing = await _repo.GetActiveTokensForUserAsync(user.Id);
            foreach (var t in existing) t.IsUsed = true;

            var token = Guid.NewGuid().ToString("N");
            _repo.AddPasswordResetToken(new PasswordResetToken
            {
                UserId    = user.Id,
                Token     = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed    = false,
                CreatedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();
            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _repo.GetValidResetTokenAsync(token);
            if (resetToken == null) return false;

            resetToken.User.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(newPassword);
            resetToken.User.FailedLoginAttempts = 0;
            resetToken.User.LockoutUntil        = null;
            resetToken.IsUsed                   = true;

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
