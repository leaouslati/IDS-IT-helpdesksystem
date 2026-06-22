using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Presentation.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notification delivery.
    /// Clients connect after JWT auth; the server pushes "ReceiveNotification" events.
    /// If a client is not connected, the push is silently dropped — the DB row remains
    /// the source of truth and the polling REST endpoints act as fallback.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        // No server-to-client methods defined here; the server pushes exclusively via IHubContext.
    }
}
