using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        // ── GET /api/lookup/categories ────────────────────────────────────────
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _lookupService.GetCategoriesAsync());
        }

        // ── GET /api/lookup/priorities ────────────────────────────────────────
        [HttpGet("priorities")]
        public async Task<IActionResult> GetPriorities()
        {
            return Ok(await _lookupService.GetPrioritiesAsync());
        }

        // ── GET /api/lookup/statuses ──────────────────────────────────────────
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            return Ok(await _lookupService.GetStatusesAsync());
        }

        // ── GET /api/lookup/departments ────────────────────────────────────────
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            return Ok(await _lookupService.GetDepartmentsAsync());
        }
    }
}
