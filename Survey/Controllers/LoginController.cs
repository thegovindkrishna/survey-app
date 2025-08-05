using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

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
        private readonly IMapper _mapper;
        private readonly ILogger<LoginController> _logger;
        private readonly IRefreshTokenService _refreshTokenService; // Declare the field

        /// <summary>
        /// Initializes a new instance of the LoginController with the specified login service.
        /// </summary>
        /// <param name="loginService">The service for handling authentication operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public LoginController(ILoginService loginService, IMapper mapper, ILogger<LoginController> logger, IRefreshTokenService refreshTokenService)
        {
            _loginService = loginService;
            _mapper = mapper;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
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
        public async Task<IActionResult> Register([FromBody] AuthRequestModel request)
        {
            _logger.LogInformation("Attempting to register user {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration failed: Email or password not provided for user {Email}", request.Email);
                return BadRequest(new { message = "Email and password are required." });
            }

            if (string.IsNullOrWhiteSpace(request.Role))
            {
                request.Role = "User"; // Default to User if no role specified
                _logger.LogInformation("No role specified for user {Email}, defaulting to 'User'", request.Email);
            }

            var result = await _loginService.Register(request.Email, request.Password, request.Role);
            
            if (!result)
            {
                if (await _loginService.GetUser(request.Email) != null)
                {
                    _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
                    return Conflict(new { message = "A user with this email already exists." });
                }
                _logger.LogError("Registration failed for user {Email} due to invalid data", request.Email);
                return BadRequest(new { message = "Invalid registration data. Please check your input." });
            }

            _logger.LogInformation("User {Email} registered successfully", request.Email);
            return Ok(new { message = "Registration successful." });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// Validates credentials and generates an authentication token.
        /// </summary>
        /// <param name="request">The login request containing email and password</param>
        /// <returns>
        /// 200 OK with JWT token and refresh token if authentication is successful,
        /// 401 Unauthorized if credentials are invalid
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestModel request)
        {
            _logger.LogInformation("Attempting to log in user {Email}", request.Email);
            var authResponse = await _loginService.Login(request.Email, request.Password);
            if (authResponse == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for user {Email}", request.Email);
                return Unauthorized(new { message = "Invalid credentials." });
            }

            _logger.LogInformation("User {Email} logged in successfully.", request.Email);
            return Ok(authResponse);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto model)
        {
            try
            {
                var (newAccessToken, newRefreshToken) = await _refreshTokenService.RefreshAccessToken(model.RefreshToken);

                return Ok(new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto model)
        {
            try
            {
                await _refreshTokenService.RevokeRefreshToken(model.RefreshToken);
                _logger.LogInformation("Refresh token revoked successfully.");
                return Ok(new { message = "Refresh token revoked" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token.");
                return StatusCode(500, new { message = "Error revoking refresh token." });
            }
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
            _logger.LogInformation("Attempting to retrieve user information for {Email}", email);
            var user = await _loginService.GetUser(email!);
            if (user == null)
            {
                _logger.LogWarning("User information retrieval failed: User {Email} not found", email);
                return NotFound(new { message = "User not found." });
            }
            _logger.LogInformation("User information retrieved successfully for {Email}", email);
            return Ok(_mapper.Map<UserDto>(user));
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
