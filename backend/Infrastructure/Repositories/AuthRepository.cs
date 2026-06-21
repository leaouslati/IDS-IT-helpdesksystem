using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserWithRoleByEmailAsync(string email) =>
            await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetActiveUserWithRoleByEmailAsync(string email) =>
            await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        public async Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(int userId) =>
            await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

        public void AddPasswordResetToken(PasswordResetToken token) =>
            _context.PasswordResetTokens.Add(token);

        public async Task<PasswordResetToken?> GetValidResetTokenAsync(string token) =>
            await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
