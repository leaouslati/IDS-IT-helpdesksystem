using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace backend.Presentation.Hubs
{
    /// <summary>
    /// Maps the SignalR connection to the authenticated user's numeric ID
    /// (the ClaimTypes.NameIdentifier claim, same as the JWT "sub" / "nameid" claim
    /// used everywhere else in this project).
    /// </summary>
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection) =>
            connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
