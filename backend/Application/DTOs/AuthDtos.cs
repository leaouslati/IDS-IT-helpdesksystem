namespace backend.Application.DTOs
{
    public class AuthLoginResult
    {
        public bool Success { get; set; }
        public LoginResponseDto? Response { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public int? MinutesRemaining { get; set; }
        public int? AttemptsRemaining { get; set; }
    }

    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class VerifyOtpResult
    {
        public bool Success { get; set; }
        public string? ResetToken { get; set; }
        public string? Error { get; set; }
        public int? AttemptsRemaining { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
