using backend.Application.DTOs;
using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<(List<NotificationDto> items, int total)> GetPagedAsync(int userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification?> FindByIdAsync(int id);
        Task MarkReadAsync(int notificationId);
        Task MarkAllReadAsync(int userId);
        Task<List<(int UserId, string Email)>> GetUserEmailsAsync(IEnumerable<int> userIds);
        Task SaveChangesAsync();
    }
}
