using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserWithRoleByEmailAsync(string email);
        Task<User?> GetActiveUserWithRoleByEmailAsync(string email);
        Task<List<PasswordResetToken>> GetActiveTokensForUserAsync(int userId);
        void AddPasswordResetToken(PasswordResetToken token);
        Task<PasswordResetToken?> GetActiveOtpForUserAsync(int userId);
        Task<PasswordResetToken?> GetVerifiedResetTokenAsync(string resetToken);
        Task SaveChangesAsync();
    }
}
