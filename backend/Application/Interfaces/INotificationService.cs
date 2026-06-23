namespace backend.Application.Interfaces
{
    public static class NotificationType
    {
        public const string TicketCreated   = "TicketCreated";
        public const string TicketAssigned  = "TicketAssigned";
        public const string TicketEscalated = "TicketEscalated";
        public const string TicketClosed    = "TicketClosed";
        public const string CommentAdded    = "CommentAdded";
        public const string AttachmentAdded = "AttachmentAdded";
    }

    public interface INotificationService
    {
        /// <summary>
        /// Creates one Notification row per recipient, optionally fires an email for
        /// email-worthy event types, and pushes via SignalR to connected clients.
        /// </summary>
        Task NotifyAsync(
            string type,
            int ticketId,
            string ticketRef,
            string ticketTitle,
            IEnumerable<int> recipientUserIds,
            string message,
            bool sendEmail = false);
    }
}
