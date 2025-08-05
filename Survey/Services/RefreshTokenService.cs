// C:\Project\survey-app\Survey\Services\RefreshTokenService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Survey.Data; // Assuming AppDbContext is in Survey.Data
using Survey.Models; // Assuming UserModel and RefreshToken are in Survey.Models
using Microsoft.EntityFrameworkCore;
using Survey.Repositories; // Assuming IUnitOfWork is here

namespace Survey.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork; 

        public RefreshTokenService(IConfiguration configuration, AppDbContext dbContext, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokens(UserModel user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenValidityInDays"])),
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };

            await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.CompleteAsync(); // Save changes

            return (accessToken, refreshToken);
        }

        public async Task<(string accessToken, string newRefreshToken)> RefreshAccessToken(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedRefreshToken == null || !storedRefreshToken.IsValid)
            {
                throw new ApplicationException("Invalid or expired refresh token.");
            }

            // Invalidate the old refresh token (optional, for rotation)
            storedRefreshToken.IsActive = false;
            storedRefreshToken.RevokedDate = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(storedRefreshToken);

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(storedRefreshToken.User);
            var newRefreshTokenValue = GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshTokenValue,
                UserId = storedRefreshToken.UserId,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenValidityInDays"])),
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };

            await _dbContext.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.CompleteAsync(); // Save changes

            return (newAccessToken, newRefreshTokenValue);
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (storedRefreshToken != null && storedRefreshToken.IsActive)
            {
                storedRefreshToken.IsActive = false;
                storedRefreshToken.RevokedDate = DateTime.UtcNow;
                _dbContext.RefreshTokens.Update(storedRefreshToken);
                await _unitOfWork.CompleteAsync(); // Save changes
            }
        }

        private string GenerateAccessToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // Add other claims as needed, e.g., roles
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenValidityInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}