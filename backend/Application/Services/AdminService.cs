using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace backend.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository     _repo;
        private readonly IDashboardRepository _dashboardRepo;
        private readonly IFileStorageService  _fileStorage;
        private readonly IConfiguration       _configuration;

        public AdminService(
            IAdminRepository repo,
            IDashboardRepository dashboardRepo,
            IFileStorageService fileStorage,
            IConfiguration configuration)
        {
            _repo          = repo;
            _dashboardRepo = dashboardRepo;
            _fileStorage   = fileStorage;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _repo.GetAllUsersWithDetailsAsync();
            return users.Select(ToDto);
        }

        public async Task<(UserListDto? user, string? error)> CreateUserAsync(CreateUserDto dto, int adminId)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName))  return (null, "First name is required.");
            if (string.IsNullOrWhiteSpace(dto.LastName))   return (null, "Last name is required.");
            if (string.IsNullOrWhiteSpace(dto.Email))      return (null, "Email is required.");
            if (string.IsNullOrWhiteSpace(dto.Password))   return (null, "Password is required.");
            if (dto.Password.Length < 8)                   return (null, "Password must be at least 8 characters.");
            if (!dto.Email.Contains('@'))                  return (null, "Invalid email address.");

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _repo.EmailExistsAsync(normalizedEmail))
                return (null, "A user with this email already exists.");
            if (!await _repo.RoleExistsAsync(dto.RoleId))
                return (null, "Invalid role selected.");
            if (dto.DepartmentId.HasValue && !await _repo.DepartmentExistsAsync(dto.DepartmentId.Value))
                return (null, "Invalid department selected.");

            var user = new User
            {
                FirstName    = dto.FirstName.Trim(),
                LastName     = dto.LastName.Trim(),
                Email        = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId       = dto.RoleId,
                DepartmentId = dto.DepartmentId,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };

            _repo.AddUser(user);
            await _repo.SaveChangesAsync();

            // If the new user is a Manager, register them as their department's manager
            var roleName = await _repo.GetRoleNameAsync(dto.RoleId);
            if (roleName == "Manager" && dto.DepartmentId.HasValue)
            {
                var dept = await _repo.GetDepartmentAsync(dto.DepartmentId.Value);
                if (dept != null)
                {
                    dept.ManagerId = user.Id;
                    await _repo.SaveChangesAsync();
                }
            }

            var created = await _repo.GetUserByIdWithDetailsAsync(user.Id);
            return (ToDto(created!), null);
        }

        public async Task<(bool success, string? error)> ToggleActivationAsync(int userId, int adminId)
        {
            if (userId == adminId)
                return (false, "You cannot deactivate your own account.");

            var user = await _repo.GetUserByIdWithDetailsAsync(userId);
            if (user == null) return (false, "User not found.");

            user.IsActive = !user.IsActive;
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> DeleteUserAsync(int userId, int adminId)
        {
            if (userId == adminId)
                return (false, "You cannot delete your own account.");

            var user = await _repo.GetUserByIdWithDetailsAsync(userId);
            if (user == null) return (false, "User not found.");

            var hasRelatedData =
                await _repo.HasCreatedTicketsAsync(userId)     ||
                await _repo.HasAssignedTicketsAsync(userId)    ||
                await _repo.HasEscalatedTicketsAsync(userId)   ||
                await _repo.HasCommentsAsync(userId)           ||
                await _repo.HasHoursLogsAsync(userId)          ||
                await _repo.HasActivityLogsAsync(userId)       ||
                await _repo.HasUploadedAttachmentsAsync(userId)||
                await _repo.HasNotificationsAsync(userId);

            if (hasRelatedData)
                return (false, "This user has existing activity and cannot be deleted. Deactivate the account instead.");

            _repo.RemoveUser(user);
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> UpdateUserRoleAsync(int userId, UpdateUserRoleDto dto, int adminId)
        {
            var user = await _repo.GetUserByIdWithDetailsAsync(userId);
            if (user == null) return (false, "User not found.");

            if (!await _repo.RoleExistsAsync(dto.RoleId))
                return (false, "Invalid role selected.");

            var previousRoleName = user.Role?.Name;
            var newRoleName      = await _repo.GetRoleNameAsync(dto.RoleId);

            user.RoleId = dto.RoleId;

            // If the user was a Manager and is being demoted, clear their dept's ManagerId
            if (previousRoleName == "Manager" && newRoleName != "Manager" && user.DepartmentId.HasValue)
            {
                var dept = await _repo.GetDepartmentAsync(user.DepartmentId.Value);
                if (dept != null && dept.ManagerId == userId)
                    dept.ManagerId = null;
            }

            // If the user is being promoted to Manager, register them as dept manager
            if (newRoleName == "Manager" && previousRoleName != "Manager" && user.DepartmentId.HasValue)
            {
                var dept = await _repo.GetDepartmentAsync(user.DepartmentId.Value);
                if (dept != null)
                    dept.ManagerId = userId;
            }

            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<RoleLookupDto>> GetRolesAsync()
        {
            var roles = await _repo.GetAllRolesAsync();
            return roles.Select(r => new RoleLookupDto { Id = r.Id, Name = r.Name });
        }

        public async Task<IEnumerable<DepartmentLookupDto>> GetDepartmentsAsync()
        {
            var depts = await _repo.GetAllDepartmentsAsync();
            return depts.Select(d => new DepartmentLookupDto { Id = d.Id, Name = d.Name });
        }

        public async Task<SystemInfoDto> GetSystemInfoAsync()
        {
            var users      = await _repo.GetAllUsersWithDetailsAsync();
            var roleCounts = users.GroupBy(u => u.Role.Name).ToDictionary(g => g.Key, g => g.Count());

            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            return new SystemInfoDto
            {
                AdminCount       = roleCounts.GetValueOrDefault("Admin"),
                ManagerCount     = roleCounts.GetValueOrDefault("Manager"),
                AgentCount       = roleCounts.GetValueOrDefault("Agent"),
                EmployeeCount    = roleCounts.GetValueOrDefault("Employee"),
                TotalUsers       = users.Count,
                TotalTickets     = await _dashboardRepo.CountAllTicketsAsync(),
                StorageUsedBytes = _fileStorage.GetTotalStorageUsedBytes(),
                AppVersion       = version,
                DatabaseName     = databaseName,
                ServerTimeUtc    = DateTime.UtcNow
            };
        }

        private static UserListDto ToDto(User u) => new()
        {
            Id           = u.Id,
            FirstName    = u.FirstName,
            LastName     = u.LastName,
            Email        = u.Email,
            Role         = u.Role.Name,
            RoleId       = u.RoleId,
            Department   = u.Department?.Name,
            DepartmentId = u.DepartmentId,
            IsActive     = u.IsActive,
            CreatedAt    = u.CreatedAt
        };
    }
}
