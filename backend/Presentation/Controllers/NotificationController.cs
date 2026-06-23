using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;

        public NotificationController(INotificationRepository repo)
        {
            _repo = repo;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // ── GET /api/notification ─────────────────────────────────────────────
        /// <summary>
        /// Returns the authenticated user's own notifications, paginated, newest first.
        /// No role — including Admin — can see another user's notifications.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, total) = await _repo.GetPagedAsync(CurrentUserId, page, pageSize);

            return Ok(new NotificationPagedDto
            {
                Items     = items,
                TotalCount = total,
                Page      = page,
                PageSize  = pageSize
            });
        }

        // ── GET /api/notification/unread-count ────────────────────────────────
        /// <summary>Returns the count of unread notifications for the authenticated user.</summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _repo.GetUnreadCountAsync(CurrentUserId);
            return Ok(new { count });
        }

        // ── PUT /api/notification/{id}/read ───────────────────────────────────
        /// <summary>Marks a single notification as read. Rejects if it belongs to another user.</summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notification = await _repo.FindByIdAsync(id);
            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            if (notification.UserId != CurrentUserId)
                return Forbid();

            await _repo.MarkReadAsync(id);
            await _repo.SaveChangesAsync();
            return Ok(new { message = "Notification marked as read." });
        }

        // ── PUT /api/notification/read-all ────────────────────────────────────
        /// <summary>Marks all of the authenticated user's unread notifications as read.</summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _repo.MarkAllReadAsync(CurrentUserId);
            return Ok(new { message = "All notifications marked as read." });
        }
    }
}
