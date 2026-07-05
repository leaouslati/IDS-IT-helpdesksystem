using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserListDto>>          GetAllUsersAsync();
        Task<(UserListDto? user, string? error)> CreateUserAsync(CreateUserDto dto, int adminId);
        Task<(bool success, string? error)>      ToggleActivationAsync(int userId, int adminId);
        Task<(bool success, string? error)>      DeleteUserAsync(int userId, int adminId);
        Task<(bool success, string? error)>      UpdateUserRoleAsync(int userId, UpdateUserRoleDto dto, int adminId);
        Task<IEnumerable<RoleLookupDto>>         GetRolesAsync();
        Task<IEnumerable<DepartmentLookupDto>>   GetDepartmentsAsync();
        Task<SystemInfoDto>                      GetSystemInfoAsync();
    }
}
