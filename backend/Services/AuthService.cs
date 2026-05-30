using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Step 1: Find the user by email
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            // Step 2: If user not found, return null
            if (user == null) return null;

            // Step 3: Check if password is correct
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordCorrect) return null;

            // Step 4: Generate JWT token
            var token = GenerateToken(user);

            // Step 5: Return the response
            return new LoginResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name
            };
        }

        private string GenerateToken(User user)
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

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["Jwt:ExpiryHours"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}