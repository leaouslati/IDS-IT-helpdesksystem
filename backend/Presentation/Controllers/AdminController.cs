using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private IActionResult ServiceError(string error) =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });

        // ── GET /api/admin/users ──────────────────────────────────────────────
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        // ── POST /api/admin/users ─────────────────────────────────────────────
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var (user, error) = await _adminService.CreateUserAsync(dto, CurrentUserId);
            if (error != null) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetUsers), user);
        }

        // ── PUT /api/admin/users/{id}/role ────────────────────────────────────
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var (success, error) = await _adminService.UpdateUserRoleAsync(id, dto, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "User role updated successfully." });
        }

        // ── PUT /api/admin/users/{id}/toggle-active ───────────────────────────
        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleActivation(int id)
        {
            var (success, error) = await _adminService.ToggleActivationAsync(id, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "User status updated successfully." });
        }

        // ── DELETE /api/admin/users/{id} ──────────────────────────────────────
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (success, error) = await _adminService.DeleteUserAsync(id, CurrentUserId);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "User deleted successfully." });
        }

        // ── GET /api/admin/roles ──────────────────────────────────────────────
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _adminService.GetRolesAsync();
            return Ok(roles);
        }

        // ── GET /api/admin/departments ────────────────────────────────────────
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var depts = await _adminService.GetDepartmentsAsync();
            return Ok(depts);
        }
    }
}
