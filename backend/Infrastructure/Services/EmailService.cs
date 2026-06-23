using backend.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace backend.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var section      = _config.GetSection("EmailSettings");
                var smtpHost     = section["SmtpHost"]         ?? "smtp.gmail.com";
                var smtpPort     = int.Parse(section["SmtpPort"] ?? "587");
                var senderEmail  = section["SenderEmail"]      ?? string.Empty;
                var senderPass   = section["SenderAppPassword"] ?? string.Empty;
                var senderName   = section["SenderName"]       ?? "IT Help Desk";

                // Skip silently when credentials are not configured (dev environments)
                if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPass))
                {
                    _logger.LogDebug("Email not configured — skipping send to {Email}", toEmail);
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body    = new TextPart("html") { Text = htmlBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Email failure must NEVER break the underlying ticket action
                _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            }
        }

        /// <summary>Builds the standard HTML email body for a ticket event.</summary>
        public static string BuildTicketEmailBody(
            string recipientName,
            string eventDescription,
            string ticketRef,
            string ticketTitle,
            string appBaseUrl,
            int ticketId)
        {
            var ticketUrl = $"{appBaseUrl.TrimEnd('/')}/tickets/{ticketId}";
            return $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;padding:20px;">
                  <h2 style="color:#2563eb;">IT Help Desk Notification</h2>
                  <p>Hello {System.Net.WebUtility.HtmlEncode(recipientName)},</p>
                  <p>{System.Net.WebUtility.HtmlEncode(eventDescription)}</p>
                  <table style="border-collapse:collapse;width:100%;margin:16px 0;">
                    <tr><td style="padding:8px;font-weight:bold;background:#f3f4f6;">Ticket</td>
                        <td style="padding:8px;">{System.Net.WebUtility.HtmlEncode(ticketRef)}</td></tr>
                    <tr><td style="padding:8px;font-weight:bold;background:#f3f4f6;">Title</td>
                        <td style="padding:8px;">{System.Net.WebUtility.HtmlEncode(ticketTitle)}</td></tr>
                  </table>
                  <a href="{ticketUrl}"
                     style="display:inline-block;padding:10px 20px;background:#2563eb;color:#fff;
                            text-decoration:none;border-radius:4px;">View Ticket</a>
                  <p style="margin-top:24px;font-size:12px;color:#9ca3af;">
                    This is an automated message from the IDS IT Help Desk system.
                  </p>
                </body>
                </html>
                """;
        }
    }
}
