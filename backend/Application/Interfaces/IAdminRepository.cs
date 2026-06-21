using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface IAdminRepository
    {
        Task<List<User>>       GetAllUsersWithDetailsAsync();
        Task<User?>            GetUserByIdWithDetailsAsync(int userId);
        Task<bool>             EmailExistsAsync(string email, int? excludeUserId = null);
        Task<bool>             RoleExistsAsync(int roleId);
        Task<bool>             DepartmentExistsAsync(int deptId);
        Task<List<Role>>       GetAllRolesAsync();
        Task<List<Department>> GetAllDepartmentsAsync();

        // AnyAsync existence checks — never load full collections into memory
        Task<bool> HasCreatedTicketsAsync(int userId);
        Task<bool> HasAssignedTicketsAsync(int userId);
        Task<bool> HasEscalatedTicketsAsync(int userId);
        Task<bool> HasCommentsAsync(int userId);
        Task<bool> HasHoursLogsAsync(int userId);
        Task<bool> HasActivityLogsAsync(int userId);
        Task<bool> HasUploadedAttachmentsAsync(int userId);
        Task<bool> HasNotificationsAsync(int userId);

        void AddUser(User user);
        void RemoveUser(User user);
        Task SaveChangesAsync();
    }
}
