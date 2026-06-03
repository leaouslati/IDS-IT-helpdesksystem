using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthLoginResult> LoginAsync(LoginRequestDto request);
        Task<string?> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
