using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _dashboardService.GetAdminDashboardAsync();
            return Ok(result);
        }

        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetManagerDashboardAsync(userId);
            return Ok(result);
        }

        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetAgentDashboardAsync(userId);
            return Ok(result);
        }

        [HttpGet("employee")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetEmployeeDashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _dashboardService.GetEmployeeDashboardAsync(userId);
            return Ok(result);
        }
    }
}
