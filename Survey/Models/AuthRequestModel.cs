using Survey.Attributes;

namespace Survey.Models
{
    public class AuthRequestModel
    {
        [RequiredBinding]
        public string Email { get; set; } = string.Empty;

        [RequiredBinding]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
