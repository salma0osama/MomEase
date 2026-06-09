using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class DeviceToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);

        public DateTime? LastUsedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}
