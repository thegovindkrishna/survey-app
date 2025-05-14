using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

namespace Survey.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            var result = await _loginService.Register(request.Email, request.Password);
            return result ? Ok("Registration successful.") : Conflict("User already exists or input is invalid.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var token = await _loginService.Login(request.Email, request.Password);
            return token != null ? Ok(new { token }) : Unauthorized("Invalid credentials.");
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _loginService.GetUser(email!);
            return user != null ? Ok(new { user.Email, user.Role }) : NotFound("User not found.");
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok("Logout successful."); // Handled client-side
        }
    }
}
