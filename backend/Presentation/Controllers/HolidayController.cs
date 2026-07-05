using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/holidays")]
    [Authorize(Roles = "Admin")]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        private IActionResult ServiceError(string error) =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });

        // ── GET /api/admin/holidays?year=2025 ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetHolidays([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var holidays = await _holidayService.GetHolidaysForYearAsync(targetYear);
            return Ok(holidays);
        }

        // ── POST /api/admin/holidays ───────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayDto dto)
        {
            var (holiday, error, isPastDate) = await _holidayService.CreateAsync(dto);
            if (error != null) return BadRequest(new { message = error });
            return Ok(new { holiday, isPastDate });
        }

        // ── PUT /api/admin/holidays/{id} ───────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHoliday(int id, [FromBody] UpdateHolidayDto dto)
        {
            var (success, error) = await _holidayService.UpdateAsync(id, dto);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Holiday updated successfully." });
        }

        // ── DELETE /api/admin/holidays/{id} ────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var (success, error) = await _holidayService.DeleteAsync(id);
            if (!success) return ServiceError(error!);
            return Ok(new { message = "Holiday deleted successfully." });
        }
    }
}
