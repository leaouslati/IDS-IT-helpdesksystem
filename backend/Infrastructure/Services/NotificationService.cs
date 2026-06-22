using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backend.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository     _repo;
        private readonly IEmailService               _email;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IConfiguration              _config;

        public NotificationService(
            INotificationRepository repo,
            IEmailService email,
            IHubContext<NotificationHub> hub,
            IConfiguration config)
        {
            _repo   = repo;
            _email  = email;
            _hub    = hub;
            _config = config;
        }

        public async Task NotifyAsync(
            string type,
            int ticketId,
            string ticketRef,
            string ticketTitle,
            IEnumerable<int> recipientUserIds,
            string message,
            bool sendEmail = false)
        {
            var recipients = recipientUserIds.Distinct().ToList();
            if (recipients.Count == 0) return;

            var title = BuildTitle(type, ticketRef);
            var now   = DateTime.UtcNow;

            // 1. Persist one row per recipient
            foreach (var userId in recipients)
            {
                await _repo.AddAsync(new Notification
                {
                    UserId    = userId,
                    TicketId  = ticketId,
                    Type      = type,
                    Title     = title,
                    Message   = message,
                    IsRead    = false,
                    CreatedAt = now
                });
            }
            await _repo.SaveChangesAsync();

            // Re-fetch the saved rows (they now have PKs) to build push DTOs
            // We build the push payload inline from what we know — avoids extra query
            var pushDto = new NotificationDto
            {
                TicketId  = ticketId,
                Type      = type,
                Title     = title,
                Message   = message,
                IsRead    = false,
                CreatedAt = now
            };

            // 2. Push via SignalR (fire-and-forget per recipient)
            foreach (var userId in recipients)
            {
                _ = _hub.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", pushDto);
            }

            // 3. Send email for email-worthy events (fire-and-forget, does not block the request)
            if (sendEmail)
            {
                _ = SendEmailsAsync(recipients, ticketId, ticketTitle, message);
            }
        }

        private async Task SendEmailsAsync(
            List<int> recipients, int ticketId, string ticketTitle, string message)
        {
            var userEmails = await _repo.GetUserEmailsAsync(recipients);
            var appBase    = _config["AppBaseUrl"] ?? "http://localhost:5173";

            foreach (var (userId, email) in userEmails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;

                // Re-use the stored message as the description in the email body
                var body = EmailService.BuildTicketEmailBody(
                    recipientName:    email,
                    eventDescription: message,
                    ticketRef:        $"(see ticket)",
                    ticketTitle:      ticketTitle,
                    appBaseUrl:       appBase,
                    ticketId:         ticketId);

                await _email.SendEmailAsync(email, $"[IT Help Desk] {ticketTitle}", body);
            }
        }

        private static string BuildTitle(string type, string ticketRef) => type switch
        {
            NotificationType.TicketCreated   => $"New Ticket {ticketRef}",
            NotificationType.TicketAssigned  => $"Ticket Assigned: {ticketRef}",
            NotificationType.TicketEscalated => $"Ticket Escalated: {ticketRef}",
            NotificationType.TicketClosed    => $"Ticket Resolved: {ticketRef}",
            NotificationType.CommentAdded    => $"New Comment on {ticketRef}",
            NotificationType.AttachmentAdded => $"File Added to {ticketRef}",
            _                                => $"Notification for {ticketRef}"
        };
    }
}
