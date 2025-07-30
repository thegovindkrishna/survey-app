// C:\Project\survey-app\Survey\Models\Dtos\RefreshTokenRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace Survey.Models.Dtos
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}