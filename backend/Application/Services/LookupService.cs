using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services
{
    public class LookupService : ILookupService
    {
        private readonly AppDbContext _context;

        public LookupService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync() =>
            await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
                .ToListAsync();

        public async Task<IEnumerable<PriorityDto>> GetPrioritiesAsync() =>
            await _context.Priorities
                .OrderBy(p => p.Id)
                .Select(p => new PriorityDto { Id = p.Id, Name = p.Name })
                .ToListAsync();

        public async Task<IEnumerable<TicketStatusDto>> GetStatusesAsync() =>
            await _context.TicketStatuses
                .Where(s => s.Name != "Pending")
                .OrderBy(s => s.Id)
                .Select(s => new TicketStatusDto { Id = s.Id, Name = s.Name })
                .ToListAsync();
    }
}
