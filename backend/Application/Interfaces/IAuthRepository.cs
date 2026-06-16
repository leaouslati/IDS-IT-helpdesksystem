using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetActiveUserWithRoleByEmailAsync(string email);
        Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(int userId);
        void AddPasswordResetToken(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidResetTokenAsync(string token);
        Task SaveChangesAsync();
    }
}
