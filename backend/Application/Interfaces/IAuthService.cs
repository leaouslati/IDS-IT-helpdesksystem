using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthLoginResult> LoginAsync(LoginRequestDto request);
        Task<bool> ForgotPasswordAsync(string email);
        Task<VerifyOtpResult> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string resetToken, string newPassword);
    }
}
