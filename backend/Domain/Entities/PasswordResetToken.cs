namespace backend.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // OTP emailed to the user for this reset request
        public string OtpHash { get; set; } = string.Empty;
        public DateTime OtpExpiresAt { get; set; }
        public int OtpAttempts { get; set; } = 0;
        public bool IsOtpVerified { get; set; } = false;

        // Short-lived session token issued once the OTP is verified,
        // used to authorize the final reset-password call
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
