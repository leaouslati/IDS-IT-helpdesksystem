using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface ITicketHoursLogRepository
    {
        Task<bool> TicketExistsAsync(int ticketId);
        Task<int?> GetAssignedAgentIdAsync(int ticketId);
        void AddLog(TicketHoursLog log);
        void AddActivityLog(ActivityLog log);
        Task<decimal> GetTotalHoursAsync(int ticketId);
        Task SaveChangesAsync();
    }
}
