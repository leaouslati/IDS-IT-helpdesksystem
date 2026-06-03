using backend.Application.DTOs;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPut("{ticketId}/assign")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignTicket(int ticketId, [FromBody] AssignTicketDto dto)
        {
            var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var manager = await _context.Users.FindAsync(managerId);
            if (manager?.DepartmentId == null)
                return BadRequest(new { message = "Manager is not assigned to a department." });

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            if (ticket.DepartmentId != manager.DepartmentId)
                return StatusCode(403, new { message = "Ticket does not belong to your department." });

            ticket.AssignedToUserId = dto.AgentUserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                UserId = dto.AgentUserId,
                Message = $"A new ticket has been assigned to you: {ticket.Title}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = managerId,
                Action = "Ticket Assigned",
                Details = $"Ticket {ticket.ReferenceNumber} assigned to agent",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Ticket assigned successfully." });
        }

        [HttpPut("{ticketId}/status")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto dto)
        {
            var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            if (ticket.AssignedToUserId != agentId)
                return StatusCode(403, new { message = "This ticket is not assigned to you." });

            var newStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(s => s.Name == dto.StatusName);

            if (newStatus == null)
                return BadRequest(new { message = "Invalid status name." });

            ticket.TicketStatusId = newStatus.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (dto.StatusName == "Resolved" || dto.StatusName == "Closed")
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = ticket.CreatedByUserId,
                    Message = $"Your ticket {ticket.ReferenceNumber} has been {dto.StatusName.ToLower()}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = agentId,
                Action = "Ticket Status Updated",
                Details = $"Ticket {ticket.ReferenceNumber} status changed to {dto.StatusName}",
                LoggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Ticket status updated to {dto.StatusName}." });
        }
    }
}
