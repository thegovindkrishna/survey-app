using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Services;
using System.Security.Claims;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for handling user authentication and registration.
    /// Provides endpoints for user registration, login, logout, and user information retrieval.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        /// <summary>
        /// Initializes a new instance of the LoginController with the specified login service.
        /// </summary>
        /// <param name="loginService">The service for handling authentication operations</param>
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// Validates input data and creates a new user account.
        /// </summary>
        /// <param name="request">The registration request containing email, password, and role</param>
        /// <returns>
        /// 200 OK with success message if registration is successful,
        /// 400 Bad Request if input is invalid,
        /// 409 Conflict if user already exists
        /// </returns>
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

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// Validates credentials and generates an authentication token.
        /// </summary>
        /// <param name="request">The login request containing email and password</param>
        /// <returns>
        /// 200 OK with JWT token and user role if authentication is successful,
        /// 401 Unauthorized if credentials are invalid
        /// </returns>
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

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// Requires a valid JWT token in the Authorization header.
        /// </summary>
        /// <returns>
        /// 200 OK with user information if authenticated,
        /// 404 Not Found if user is not found
        /// </returns>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = await _loginService.GetUser(email!);
            return user != null ? Ok(new { user.Email, user.Role }) : NotFound(new { message = "User not found." });
        }

        /// <summary>
        /// Logs out the currently authenticated user.
        /// In a stateless JWT implementation, this endpoint primarily serves as a placeholder.
        /// The actual logout is handled client-side by removing the JWT token.
        /// </summary>
        /// <returns>200 OK with logout success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful." });
        }
    }
}
