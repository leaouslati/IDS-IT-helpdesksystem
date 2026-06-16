using AutoMapper;
using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Services
{
    public class TicketHoursLogService : ITicketHoursLogService
    {
        private readonly ITicketHoursLogRepository _repo;
        private readonly IMapper _mapper;

        public TicketHoursLogService(ITicketHoursLogRepository repo, IMapper mapper)
        {
            _repo   = repo;
            _mapper = mapper;
        }

        public async Task<(HoursLogResponseDto? result, string? error)> LogHoursAsync(
            int ticketId, LogHoursDto dto, int agentId)
        {
            if (dto.HoursWorked <= 0)
                return (null, "Hours worked must be greater than 0.");
            if (dto.HoursWorked > 24)
                return (null, "Cannot log more than 24 hours in a single entry.");

            if (!await _repo.TicketExistsAsync(ticketId))
                return (null, "Ticket not found.");

            var assignedAgentId = await _repo.GetAssignedAgentIdAsync(ticketId);
            if (assignedAgentId != agentId)
                return (null, "You can only log hours on tickets assigned to you.");

            var log = new TicketHoursLog
            {
                TicketId    = ticketId,
                AgentId     = agentId,
                HoursWorked = dto.HoursWorked,
                LogDate     = dto.LogDate.Date,
                Notes       = dto.Notes?.Trim()
            };

            _repo.AddLog(log);

            _repo.AddActivityLog(new ActivityLog
            {
                UserId   = agentId,
                TicketId = ticketId,
                Action   = "Hours Logged",
                Details  = $"Agent logged {dto.HoursWorked:F2}h on {dto.LogDate:yyyy-MM-dd}"
                           + (string.IsNullOrWhiteSpace(dto.Notes) ? string.Empty : $": {dto.Notes}"),
                LoggedAt = DateTime.UtcNow
            });

            await _repo.SaveChangesAsync();

            // Map the saved entity to the response DTO
            // Agent navigation property is not loaded here; we set AgentName via a lightweight approach
            var response = new HoursLogResponseDto
            {
                TicketHoursLogId = log.TicketHoursLogId,
                TicketId         = log.TicketId,
                AgentName        = string.Empty, // populated by controller from claims
                HoursWorked      = log.HoursWorked,
                LogDate          = log.LogDate,
                Notes            = log.Notes
            };

            return (response, null);
        }
    }
}
