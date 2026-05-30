using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Validate that email and password are not empty
            if (string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var result = await _authService.LoginAsync(request);

            // If service returns null, credentials are wrong
            if (result == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(result);
        }
    }
}