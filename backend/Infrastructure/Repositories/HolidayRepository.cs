using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly AppDbContext _context;

        public HolidayRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Holiday>> GetAllAsync() =>
            await _context.Holidays.OrderBy(h => h.Date).ToListAsync();

        public async Task<Holiday?> FindAsync(int id) =>
            await _context.Holidays.FindAsync(id);

        public void Add(Holiday holiday) => _context.Holidays.Add(holiday);

        public void Remove(Holiday holiday) => _context.Holidays.Remove(holiday);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
