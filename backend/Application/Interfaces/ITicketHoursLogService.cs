using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface ITicketHoursLogService
    {
        Task<(HoursLogResponseDto? result, string? error)> LogHoursAsync(
            int ticketId, LogHoursDto dto, int agentId);
    }
}
