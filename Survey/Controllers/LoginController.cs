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
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            if (string.IsNullOrWhiteSpace(request.Role))
            {
                request.Role = "User"; // Default to User if no role specified
            }

            var result = await _loginService.Register(request.Email, request.Password, request.Role);
            
            if (!result)
            {
                if (await _loginService.GetUser(request.Email) != null)
                {
                    return Conflict(new { message = "A user with this email already exists." });
                }
                return BadRequest(new { message = "Invalid registration data. Please check your input." });
            }

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var token = await _loginService.Login(request.Email, request.Password);
            if (token == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var user = await _loginService.GetUser(request.Email);

            // Ensure role is always present and never null
            var role = user?.Role ?? "User";

            return Ok(new { token = token, role = role });
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _loginService.GetUser(email!);
            return user != null ? Ok(new { user.Email, user.Role }) : NotFound(new { message = "User not found." });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful." });
        }
    }
}
