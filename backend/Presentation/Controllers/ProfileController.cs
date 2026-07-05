using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // ── GET /api/profile ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _profileService.GetProfileAsync(CurrentUserId);
            if (profile == null) return NotFound(new { message = "User not found." });
            return Ok(profile);
        }

        // ── PUT /api/profile ──────────────────────────────────────────────────
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var (success, errors) = await _profileService.UpdateProfileAsync(CurrentUserId, dto);
            if (!success) return BadRequest(new { errors });
            return Ok(new { message = "Profile updated successfully." });
        }

        // ── PUT /api/profile/change-password ──────────────────────────────────
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var (success, error) = await _profileService.ChangePasswordAsync(CurrentUserId, dto);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "Password changed successfully." });
        }

        // ── POST /api/profile/picture ─────────────────────────────────────────
        [HttpPost("picture")]
        public async Task<IActionResult> UploadPicture(IFormFile? file)
        {
            var (url, error) = await _profileService.UploadPictureAsync(CurrentUserId, file);
            if (error != null) return BadRequest(new { message = error });
            return Ok(new { profilePictureUrl = url });
        }

        // ── DELETE /api/profile/picture ───────────────────────────────────────
        [HttpDelete("picture")]
        public async Task<IActionResult> DeletePicture()
        {
            var (success, error) = await _profileService.DeletePictureAsync(CurrentUserId);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "Profile picture removed." });
        }

        // ── GET /api/profile/picture ───────────────────────────────────────────
        // Streams the caller's own profile picture (blob fetch, same pattern as ticket attachments)
        [HttpGet("picture")]
        public async Task<IActionResult> GetPicture()
        {
            var (stream, contentType, error) = await _profileService.GetPictureStreamAsync(CurrentUserId);
            if (error != null || stream == null) return NotFound(new { message = error });
            return File(stream, contentType);
        }
    }
}
