namespace backend.Application.Interfaces
{
    public static class NotificationType
    {
        public const string TicketCreated    = "TicketCreated";
        public const string TicketAssigned   = "TicketAssigned";
        public const string TicketEscalated  = "TicketEscalated";
        public const string TicketClosed     = "TicketClosed";
        public const string CommentAdded     = "CommentAdded";
        public const string AttachmentAdded  = "AttachmentAdded";
        // Admin-only system events (no ticket context)
        public const string CriticalTicket   = "CriticalTicket";
        public const string EscalationAlert  = "EscalationAlert";
        public const string AccountLocked    = "AccountLocked";
    }

    public interface INotificationService
    {
        /// <summary>
        /// Creates one Notification row per recipient, optionally fires an email,
        /// and pushes via SignalR. Use for ticket-related events.
        /// </summary>
        Task NotifyAsync(
            string type,
            int ticketId,
            string ticketRef,
            string ticketTitle,
            IEnumerable<int> recipientUserIds,
            string message,
            bool sendEmail = false);

        /// <summary>
        /// Sends a system-level notification to all active Admin users.
        /// No ticket context required — used for security/ops alerts.
        /// </summary>
        Task NotifyAdminsAsync(string type, string title, string message);
    }
}
