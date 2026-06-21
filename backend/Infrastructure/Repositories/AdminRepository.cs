using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersWithDetailsAsync() =>
            await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .ToListAsync();

        public async Task<User?> GetUserByIdWithDetailsAsync(int userId) =>
            await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null) =>
            await _context.Users.AnyAsync(u =>
                u.Email == email &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

        public async Task<bool> RoleExistsAsync(int roleId) =>
            await _context.Roles.AnyAsync(r => r.Id == roleId);

        public async Task<bool> DepartmentExistsAsync(int deptId) =>
            await _context.Departments.AnyAsync(d => d.Id == deptId);

        public async Task<List<Role>> GetAllRolesAsync() =>
            await _context.Roles.OrderBy(r => r.Name).ToListAsync();

        public async Task<List<Department>> GetAllDepartmentsAsync() =>
            await _context.Departments.OrderBy(d => d.Name).ToListAsync();

        // AnyAsync checks — never load collections
        public async Task<bool> HasCreatedTicketsAsync(int userId) =>
            await _context.Tickets.AnyAsync(t => t.CreatedByUserId == userId);

        public async Task<bool> HasAssignedTicketsAsync(int userId) =>
            await _context.Tickets.AnyAsync(t => t.AssignedToUserId == userId);

        public async Task<bool> HasEscalatedTicketsAsync(int userId) =>
            await _context.Tickets.AnyAsync(t => t.EscalatedByUserId == userId);

        public async Task<bool> HasCommentsAsync(int userId) =>
            await _context.TicketComments.AnyAsync(c => c.UserId == userId);

        public async Task<bool> HasHoursLogsAsync(int userId) =>
            await _context.TicketHoursLogs.AnyAsync(h => h.AgentId == userId);

        public async Task<bool> HasActivityLogsAsync(int userId) =>
            await _context.ActivityLogs.AnyAsync(a => a.UserId == userId);

        public async Task<bool> HasUploadedAttachmentsAsync(int userId) =>
            await _context.TicketAttachments.AnyAsync(a => a.UploadedByUserId == userId);

        public async Task<bool> HasNotificationsAsync(int userId) =>
            await _context.Notifications.AnyAsync(n => n.UserId == userId);

        public void AddUser(User user) => _context.Users.Add(user);

        public void RemoveUser(User user) => _context.Users.Remove(user);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
