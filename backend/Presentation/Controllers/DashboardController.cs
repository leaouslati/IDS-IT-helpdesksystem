using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>System-wide admin dashboard. days=30 scopes ticket counts to the last N days.</summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] int days = 30)
        {
            var result = await _dashboardService.GetAdminDashboardAsync(days);
            return Ok(result);
        }

        /// <summary>Manager dashboard scoped to the manager's department.</summary>
        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerDashboard([FromQuery] int days = 30)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetManagerDashboardAsync(userId, days);
            return Ok(result);
        }

        /// <summary>Agent dashboard scoped to the agent's assigned tickets.</summary>
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentDashboard([FromQuery] int days = 30)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetAgentDashboardAsync(userId, days);
            return Ok(result);
        }

        /// <summary>Employee dashboard showing the employee's own ticket counts within the window.</summary>
        [HttpGet("employee")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetEmployeeDashboard([FromQuery] int days = 30)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetEmployeeDashboardAsync(userId, days);
            return Ok(result);
        }
    }
}
