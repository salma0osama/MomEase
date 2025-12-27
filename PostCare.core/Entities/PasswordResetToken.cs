using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.Now >= ExpiresAt;

        [NotMapped]
        public bool IsValid => !IsUsed && !IsExpired;
    }
}
