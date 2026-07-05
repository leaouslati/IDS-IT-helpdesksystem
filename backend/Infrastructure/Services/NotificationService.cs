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

            // 1. Persist one row per recipient, keeping track of each entity
            //    so EF Core populates the PK after SaveChangesAsync.
            var saved = new List<Notification>(recipients.Count);
            foreach (var userId in recipients)
            {
                var n = new Notification
                {
                    UserId    = userId,
                    TicketId  = ticketId,
                    Type      = type,
                    Title     = title,
                    Message   = message,
                    IsRead    = false,
                    CreatedAt = now
                };
                await _repo.AddAsync(n);
                saved.Add(n);
            }
            await _repo.SaveChangesAsync();
            // EF Core has now populated n.Id on every saved entity.

            // 2. Push via SignalR (fire-and-forget per recipient, each with its own DB Id)
            foreach (var n in saved)
            {
                var pushDto = new NotificationDto
                {
                    Id        = n.Id,
                    TicketId  = ticketId,
                    Type      = type,
                    Title     = title,
                    Message   = message,
                    IsRead    = false,
                    CreatedAt = now
                };
                _ = _hub.Clients.User(n.UserId.ToString())
                    .SendAsync("ReceiveNotification", pushDto);
            }

            // 3. Send email for email-worthy events.
            // Resolve emails NOW (inside the current scope/DbContext) then hand off
            // only the SMTP work as fire-and-forget — no DbContext is touched after this point.
            if (sendEmail)
            {
                var userEmails = await _repo.GetUserEmailsAsync(recipients);
                var appBase    = _config["AppBaseUrl"] ?? "http://localhost:8080";
                _ = SendEmailsAsync(userEmails, appBase, ticketId, ticketRef, ticketTitle, message);
            }
        }

        private async Task SendEmailsAsync(
            List<(int UserId, string Email, string Name)> userEmails,
            string appBase,
            int ticketId,
            string ticketRef,
            string ticketTitle,
            string message)
        {
            foreach (var (_, email, name) in userEmails)
            {
                if (string.IsNullOrWhiteSpace(email)) continue;

                var body = EmailService.BuildTicketEmailBody(
                    recipientName:    name,
                    eventDescription: message,
                    ticketRef:        ticketRef,
                    ticketTitle:      ticketTitle,
                    appBaseUrl:       appBase,
                    ticketId:         ticketId);

                await _email.SendEmailAsync(email, $"[IT Help Desk] {ticketRef}: {ticketTitle}", body);
            }
        }

        public async Task NotifyAdminsAsync(string type, string title, string message)
        {
            var adminIds = await _repo.GetAdminUserIdsAsync();
            if (adminIds.Count == 0) return;

            var now   = DateTime.UtcNow;
            var saved = new List<Notification>(adminIds.Count);
            foreach (var userId in adminIds)
            {
                var n = new Notification
                {
                    UserId    = userId,
                    TicketId  = null,
                    Type      = type,
                    Title     = title,
                    Message   = message,
                    IsRead    = false,
                    CreatedAt = now
                };
                await _repo.AddAsync(n);
                saved.Add(n);
            }
            await _repo.SaveChangesAsync();

            foreach (var n in saved)
            {
                _ = _hub.Clients.User(n.UserId.ToString())
                    .SendAsync("ReceiveNotification", new NotificationDto
                    {
                        Id        = n.Id,
                        TicketId  = null,
                        Type      = type,
                        Title     = title,
                        Message   = message,
                        IsRead    = false,
                        CreatedAt = now
                    });
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
