using backend.Application.DTOs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            var result = await _authService.LoginAsync(request);

            if (result.Success)
                return Ok(result.Response);

            if (result.ErrorCode == "ACCOUNT_LOCKED")
                return StatusCode(423, new
                {
                    error = "ACCOUNT_LOCKED",
                    message = $"Account locked. Try again in {result.MinutesRemaining} minute(s).",
                    minutesRemaining = result.MinutesRemaining
                });

            return Unauthorized(new
            {
                error = "INVALID_CREDENTIALS",
                message = "Invalid email or password.",
                attemptsRemaining = result.AttemptsRemaining
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Email is required." });

            var token = await _authService.ForgotPasswordAsync(request.Email);

            if (token == null)
                return NotFound(new { message = "No active account found with that email address." });

            return Ok(new { resetToken = token, expiresMinutes = 15 });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new { message = "Token and new password are required." });

            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

            if (!success)
                return BadRequest(new { message = "Reset link is invalid or has expired." });

            return Ok(new { message = "Password reset successfully." });
        }
    }
}
