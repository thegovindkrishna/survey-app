// C:\Project\survey-app\Survey\Services\IRefreshTokenService.cs
using System.Threading.Tasks;
using Survey.Models; // Assuming UserModel and RefreshToken are in Survey.Models
using Survey.Models.Dtos; // Assuming AuthRequestModel is in Survey.Models.Dtos

namespace Survey.Services
{
    public interface IRefreshTokenService
    {
        Task<(string accessToken, string refreshToken)> GenerateTokens(UserModel user);
        Task<(string accessToken, string newRefreshToken)> RefreshAccessToken(string refreshToken);
        Task RevokeRefreshToken(string refreshToken);
    }
}