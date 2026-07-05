using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindUserWithDetailsAsync(int userId) =>
            await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<bool> EmailExistsAsync(string email, int excludeUserId) =>
            await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);

        public async Task<bool> DepartmentExistsAsync(int deptId) =>
            await _context.Departments.AnyAsync(d => d.Id == deptId);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
