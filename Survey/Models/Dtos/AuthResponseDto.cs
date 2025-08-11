// C:\Project\survey-app\Survey\Models\Dtos\AuthResponseDto.cs
namespace Survey.Models.Dtos
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        // Add other user details if you want to return them with the tokens
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}