using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification) =>
            await _context.Notifications.AddAsync(notification);

        public async Task<(List<NotificationDto> items, int total)> GetPagedAsync(
            int userId, int page, int pageSize)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id        = n.Id,
                    TicketId  = n.TicketId,
                    Type      = n.Type,
                    Title     = n.Title,
                    Message   = n.Message,
                    IsRead    = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> GetUnreadCountAsync(int userId) =>
            await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<Notification?> FindByIdAsync(int id) =>
            await _context.Notifications.FindAsync(id);

        public async Task MarkReadAsync(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n != null) n.IsRead = true;
        }

        public async Task MarkAllReadAsync(int userId) =>
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public async Task<List<int>> GetAdminUserIdsAsync() =>
            await _context.Users
                .Where(u => u.IsActive && u.Role.Name == "Admin")
                .Select(u => u.Id)
                .ToListAsync();

        public async Task<List<(int UserId, string Email, string Name)>> GetUserEmailsAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.ToList();
            return (await _context.Users
                .Where(u => ids.Contains(u.Id) && u.IsActive)
                .Select(u => new { u.Id, u.Email, Name = u.FirstName + " " + u.LastName })
                .ToListAsync())
                .Select(u => (u.Id, u.Email, u.Name))
                .ToList();
        }

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
