using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public interface IProfileRepository
    {
        Task<User?> FindUserWithDetailsAsync(int userId);
        Task<bool>  EmailExistsAsync(string email, int excludeUserId);
        Task<bool>  DepartmentExistsAsync(int deptId);
        Task SaveChangesAsync();
    }
}
