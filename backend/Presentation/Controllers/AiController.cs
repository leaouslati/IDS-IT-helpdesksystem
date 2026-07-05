using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IGroqService _groqService;

        public AiController(IGroqService groqService)
        {
            _groqService = groqService;
        }

        // ── POST /api/ai/analyze-ticket ────────────────────────────────────────
        [HttpPost("analyze-ticket")]
        public async Task<IActionResult> AnalyzeTicket([FromBody] AnalyzeTicketRequestDto dto)
        {
            var description = dto.Description?.Trim() ?? string.Empty;

            if (description.Length < 10)
                return BadRequest(new { message = "Description must be at least 10 characters long." });
            if (description.Length > 2000)
                return BadRequest(new { message = "Description must not exceed 2000 characters." });

            var result = await _groqService.AnalyzeTicketAsync(description);

            return Ok(new AnalyzeTicketResponseDto
            {
                SuggestedCategory  = result.SuggestedCategory,
                SuggestedPriority  = result.SuggestedPriority,
                CategoryConfidence = result.CategoryConfidence,
                PriorityConfidence = result.PriorityConfidence
            });
        }
    }
}
