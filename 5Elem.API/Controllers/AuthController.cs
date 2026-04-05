using _5Elem.API.Services.Interfaces;
using _5Elem.Shared.Models;

using Microsoft.AspNetCore.Mvc;

namespace _5Elem.API.Controllers
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Username, request.Password);
                if (result == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                Console.WriteLine("qwe");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("qwerty " + ex);
                return ControllerBase.Empty;
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
            if (result == null)
                return BadRequest(new { message = "Username already exists" });

            return Ok(result);
        }
    }
}
