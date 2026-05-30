using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    /// <summary>
    /// Daily reminder for mother to track child's data
    /// </summary>
    public class DailyTrackingReminder
    {
        [Key]
        public int ReminderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public DateTime ReminderDate { get; set; }

        [Required]
        public bool IsSent { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; }

        [ForeignKey(nameof(ChildId))]
        public virtual Child Child { get; set; }
    }
}
