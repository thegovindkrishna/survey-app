using Survey.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Survey.Data;
using Survey.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using BCrypt.Net; // Add this for password hashing
using Survey.Models.Dtos; // Add this for AuthResponseDto

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for user authentication and registration.
    /// Handles user registration, login, and user retrieval operations with JWT token generation.
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly ILogger<LoginService> _logger;
        private readonly IRefreshTokenService _refreshTokenService; // Declare the field
        private static readonly string[] ValidRoles = { "User", "Admin" };

        /// <summary>
        /// Initializes a new instance of the LoginService with the specified database context and configuration.
        /// </summary>
        /// <param name="context">The database context for user operations</param>
        /// <param name="config">The configuration containing JWT settings</param>
        /// <param name="logger">The logger instance</param>
        public LoginService(IUnitOfWork unitOfWork, IConfiguration config, ILogger<LoginService> logger, IRefreshTokenService refreshTokenService)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// Validates email and password, checks for existing users, and validates the role.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="password">The user's password</param>
        /// <param name="role">The user's role (defaults to "User")</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        public async Task<bool> Register(string email, string password, string role = "User")
        {
            _logger.LogInformation("Attempting to register user {Email} with role {Role}", email, role);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Registration failed for {Email}: Email or password not provided.", email);
                return false;
            }

            if (await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Email == email) != null)
            {
                _logger.LogWarning("Registration failed for {Email}: User already exists.", email);
                return false;
            }

            // Validate role
            if (!ValidRoles.Contains(role))
            {
                _logger.LogWarning("Registration failed for {Email}: Invalid role '{Role}' provided.", email, role);
                return false;
            }

            CreatePasswordHash(password, out string passwordHash);
            var user = new UserModel { Email = email, PasswordHash = passwordHash, Role = role };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("User {Email} registered successfully with role {Role}", email, role);
            return true;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// Validates email and password, then creates a JWT token with user claims.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="password">The user's password</param>
        /// <returns>An AuthResponseDto containing JWT and refresh tokens if authentication is successful, null otherwise</returns>
        public async Task<AuthResponseDto?> Login(string email, string password)
        {
            _logger.LogInformation("Attempting to log in user {Email}", email);
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for {Email}: Invalid credentials.", email);
                return null;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Use NameIdentifier for UserId
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenValidityInMinutes"])),
                signingCredentials: creds
            );

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            var (generatedAccessToken, generatedRefreshToken) = await _refreshTokenService.GenerateTokens(user);
            var refreshToken = generatedRefreshToken; 

            _logger.LogInformation("User {Email} logged in successfully.", email);
            return new AuthResponseDto
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken,
                Email = user.Email,
                Username = user.Email,
                Role = user.Role // Assuming email is used as username
            };
        }

        private void CreatePasswordHash(string password, out string passwordHash)
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <returns>The user object if found, null otherwise</returns>
        public async Task<UserModel?> GetUser(string email)
        {
            _logger.LogInformation("Retrieving user by email: {Email}", email);
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found.", email);
            }
            else
            {
                _logger.LogInformation("User with email {Email} retrieved successfully.", email);
            }
            return user;
        }
    }
}
