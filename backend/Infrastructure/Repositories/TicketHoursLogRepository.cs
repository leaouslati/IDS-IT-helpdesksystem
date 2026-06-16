using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class TicketHoursLogRepository : ITicketHoursLogRepository
    {
        private readonly AppDbContext _context;

        public TicketHoursLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TicketExistsAsync(int ticketId) =>
            await _context.Tickets.AnyAsync(t => t.Id == ticketId);

        public async Task<int?> GetAssignedAgentIdAsync(int ticketId) =>
            await _context.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => t.AssignedToUserId)
                .FirstOrDefaultAsync();

        public void AddLog(TicketHoursLog log) => _context.TicketHoursLogs.Add(log);

        public void AddActivityLog(ActivityLog log) => _context.ActivityLogs.Add(log);

        public async Task<decimal> GetTotalHoursAsync(int ticketId) =>
            await _context.TicketHoursLogs
                .Where(h => h.TicketId == ticketId)
                .SumAsync(h => (decimal?)h.HoursWorked) ?? 0m;

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
