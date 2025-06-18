using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Survey.Data;
using Survey.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for user authentication and registration.
    /// Handles user registration, login, and user retrieval operations with JWT token generation.
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private static readonly string[] ValidRoles = { "User", "Admin" };

        /// <summary>
        /// Initializes a new instance of the LoginService with the specified database context and configuration.
        /// </summary>
        /// <param name="context">The database context for user operations</param>
        /// <param name="config">The configuration containing JWT settings</param>
        public LoginService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// Validates email and password, checks for existing users, and validates the role.
        /// </summary>
        /// <param name="email">The user's email address (used as username)</param>
        /// <param name="password">The user's password</param>
        /// <param name="role">The user's role (defaults to "User")</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        public async Task<bool> Register(string email, string password, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return false;

            // Validate role
            if (!ValidRoles.Contains(role))
                return false;

            var user = new User { Email = email, Password = password, Role = role };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// Validates email and password, then creates a JWT token with user claims.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="password">The user's password</param>
        /// <returns>A JWT token if authentication is successful, null otherwise</returns>
        public async Task<string?> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null) return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <returns>The user object if found, null otherwise</returns>
        public async Task<User?> GetUser(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
