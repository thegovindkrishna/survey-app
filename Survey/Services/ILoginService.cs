using Survey.Models;

namespace Survey.Services
{
    /// <summary>
    /// Service interface for user authentication and registration.
    /// Handles user registration, login, and user retrieval operations.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="email">The user's email address (used as username)</param>
        /// <param name="password">The user's password</param>
        /// <param name="role">The user's role (defaults to "User")</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        Task<bool> Register(string email, string password, string role = "User");

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="password">The user's password</param>
        /// <returns>A JWT token if authentication is successful, null otherwise</returns>
        Task<string?> Login(string email, string password);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <returns>The user object if found, null otherwise</returns>
        Task<UserModel?> GetUser(string email);
    }
}
