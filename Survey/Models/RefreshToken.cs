// C:\Project\survey-app\Survey\Models\RefreshToken.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true; // Indicates if the token is still valid

        [ForeignKey("User")]
        public int UserId { get; set; } // Assuming UserId is int, adjust if Guid

        public UserModel User { get; set; } // Navigation property

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsRevoked => RevokedDate != null;
        public bool IsValid => IsActive && !IsExpired && !IsRevoked;
    }
}