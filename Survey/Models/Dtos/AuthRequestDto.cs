namespace Survey.Models.Dtos
{
    public class AuthRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; } = "User";
    }
}
