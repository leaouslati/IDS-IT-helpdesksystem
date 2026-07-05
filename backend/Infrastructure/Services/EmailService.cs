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

        public static string BuildTicketEmailBody(
            string recipientName,
            string eventDescription,
            string ticketRef,
            string ticketTitle,
            string appBaseUrl,
            int ticketId)
        {
            var ticketUrl = $"{appBaseUrl.TrimEnd('/')}/tickets/{ticketId}";
            var name      = System.Net.WebUtility.HtmlEncode(recipientName);
            var desc      = System.Net.WebUtility.HtmlEncode(eventDescription);
            var refEnc    = System.Net.WebUtility.HtmlEncode(ticketRef);
            var titleEnc  = System.Net.WebUtility.HtmlEncode(ticketTitle);

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
                <body style="margin:0;padding:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9;padding:32px 16px;">
                    <tr><td align="center">
                      <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.07);">

                        <!-- Header bar -->
                        <tr>
                          <td style="background:linear-gradient(135deg,#0f172a 0%,#1e293b 100%);padding:28px 32px;">
                            <table width="100%" cellpadding="0" cellspacing="0">
                              <tr>
                                <td>
                                  <span style="display:inline-block;background:#14b8a6;border-radius:6px;padding:4px 10px;font-size:11px;font-weight:700;color:#fff;letter-spacing:0.5px;text-transform:uppercase;">IT Help Desk</span>
                                </td>
                              </tr>
                              <tr>
                                <td style="padding-top:12px;">
                                  <p style="margin:0;font-size:20px;font-weight:700;color:#ffffff;">Ticket Notification</p>
                                  <p style="margin:4px 0 0;font-size:13px;color:#94a3b8;">IDS Internal Support System</p>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- Body -->
                        <tr>
                          <td style="padding:32px;">
                            <p style="margin:0 0 8px;font-size:15px;color:#0f172a;">Hello <strong>{name}</strong>,</p>
                            <p style="margin:0 0 24px;font-size:14px;color:#475569;line-height:1.6;">{desc}</p>

                            <!-- Ticket details card -->
                            <table width="100%" cellpadding="0" cellspacing="0" style="background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden;margin-bottom:28px;">
                              <tr style="background:#f1f5f9;">
                                <td colspan="2" style="padding:10px 16px;font-size:11px;font-weight:700;color:#64748b;text-transform:uppercase;letter-spacing:0.5px;border-bottom:1px solid #e2e8f0;">
                                  Ticket Details
                                </td>
                              </tr>
                              <tr>
                                <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#64748b;width:30%;border-bottom:1px solid #f1f5f9;">Reference</td>
                                <td style="padding:10px 16px;font-size:13px;color:#0f172a;font-weight:600;border-bottom:1px solid #f1f5f9;">{refEnc}</td>
                              </tr>
                              <tr>
                                <td style="padding:10px 16px;font-size:13px;font-weight:600;color:#64748b;">Title</td>
                                <td style="padding:10px 16px;font-size:13px;color:#0f172a;">{titleEnc}</td>
                              </tr>
                            </table>

                            <!-- CTA button -->
                            <table cellpadding="0" cellspacing="0">
                              <tr>
                                <td style="border-radius:8px;background:#14b8a6;">
                                  <a href="{ticketUrl}"
                                     style="display:inline-block;padding:12px 28px;font-size:14px;font-weight:700;color:#ffffff;text-decoration:none;border-radius:8px;letter-spacing:0.2px;">
                                    View Ticket &rarr;
                                  </a>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>

                        <!-- Footer -->
                        <tr>
                          <td style="background:#f8fafc;border-top:1px solid #e2e8f0;padding:16px 32px;">
                            <p style="margin:0;font-size:12px;color:#94a3b8;line-height:1.5;">
                              This is an automated message from the IDS IT Help Desk system. Please do not reply to this email.
                            </p>
                          </td>
                        </tr>

                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;
        }
    }
}
