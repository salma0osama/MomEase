using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class RefreshToken
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

        public DateTime? RevokedAt { get; set; }

        [MaxLength(50)]
        public string CreatedByIp { get; set; }

        [MaxLength(50)]
        public string RevokedByIp { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.Now >= ExpiresAt;

        [NotMapped]
        public bool IsRevoked => RevokedAt != null;

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    
}
}
