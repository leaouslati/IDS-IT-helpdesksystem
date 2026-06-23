namespace backend.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an HTML email. Never throws — SMTP failures are logged and swallowed
        /// so the underlying business action always succeeds.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
