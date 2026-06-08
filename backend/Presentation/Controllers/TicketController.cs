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
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private int    CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private string CurrentRole   => User.FindFirst(ClaimTypes.Role)!.Value;

        // Returns 404 when the error is "not found", otherwise 400
        private IActionResult ServiceError(string error) =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });

        // ── GET /api/ticket ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _ticketService.GetTicketsAsync(CurrentUserId, CurrentRole);
            return Ok(tickets);
        }

        // ── GET /api/ticket/{id} ──────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id, CurrentUserId, CurrentRole);
            if (ticket == null) return NotFound(new { message = "Ticket not found." });
            return Ok(ticket);
        }

        // ── POST /api/ticket ──────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var (ticket, error) = await _ticketService.CreateTicketAsync(dto, CurrentUserId);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket!.Id }, ticket);
        }

        // ── PUT /api/ticket/{id} ──────────────────────────────────────────────
        // Employee only; enforced in service (Open status, own ticket)
        [HttpPut("{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
        {
            var (success, error) = await _ticketService.UpdateTicketAsync(id, dto, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Ticket updated successfully." });
        }

        // ── DELETE /api/ticket/{id} ───────────────────────────────────────────
        // Employee (own ticket) or Manager (dept ticket); Open status only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var (success, error) = await _ticketService.DeleteTicketAsync(id, CurrentUserId, CurrentRole);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Ticket deleted successfully." });
        }

        // ── PUT /api/ticket/{ticketId}/assign ─────────────────────────────────
        // Manager only; max 3 active tickets per agent enforced in service
        [HttpPut("{ticketId}/assign")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignTicket(int ticketId, [FromBody] AssignTicketDto dto)
        {
            var (success, error) = await _ticketService.AssignTicketAsync(ticketId, dto, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Ticket assigned successfully." });
        }

        // ── PUT /api/ticket/{ticketId}/status ─────────────────────────────────
        // Agent only; cannot revert to Open or change Resolved/Closed
        [HttpPut("{ticketId}/status")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto dto)
        {
            var (success, error) = await _ticketService.UpdateStatusAsync(ticketId, dto, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = $"Ticket status updated to {dto.StatusName}." });
        }

        // ── PUT /api/ticket/{ticketId}/escalate ───────────────────────────────
        // Agent (assigned ticket) or Manager (dept ticket); reason is mandatory
        [HttpPut("{ticketId}/escalate")]
        [Authorize(Roles = "Agent,Manager")]
        public async Task<IActionResult> EscalateTicket(int ticketId, [FromBody] EscalateTicketDto dto)
        {
            var (success, error) = await _ticketService.EscalateTicketAsync(ticketId, dto, CurrentUserId, CurrentRole);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Ticket escalated successfully." });
        }

        // ── POST /api/ticket/{ticketId}/comment ───────────────────────────────
        // All roles; not allowed on Closed tickets
        [HttpPost("{ticketId}/comment")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddCommentDto dto)
        {
            var (comment, error) = await _ticketService.AddCommentAsync(ticketId, dto, CurrentUserId, CurrentRole);
            if (error != null) return ServiceError(error);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticketId }, comment);
        }

        // ── GET /api/ticket/{ticketId}/comments ───────────────────────────────
        [HttpGet("{ticketId}/comments")]
        public async Task<IActionResult> GetComments(int ticketId)
        {
            var comments = await _ticketService.GetCommentsAsync(ticketId, CurrentUserId, CurrentRole);
            return Ok(comments);
        }

        // ── POST /api/ticket/{ticketId}/attachments ───────────────────────────
        // Accepts multipart/form-data; file field name must be "file"
        [HttpPost("{ticketId}/attachments")]
        [Authorize(Roles = "Employee,Agent,Manager")]
        public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile? file)
        {
            var (attachment, error) = await _ticketService.UploadAttachmentAsync(ticketId, file, CurrentUserId, CurrentRole);
            if (error != null) return ServiceError(error);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticketId }, attachment);
        }

        // ── GET /api/ticket/{ticketId}/agents-availability ────────────────────
        // Returns agents in the ticket's department with their active ticket counts
        [HttpGet("{ticketId}/agents-availability")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAgentsAvailability(int ticketId)
        {
            var (agents, error) = await _ticketService.GetAgentsAvailabilityAsync(ticketId, CurrentUserId);
            if (error != null) return ServiceError(error);
            return Ok(agents);
        }
    }
}
