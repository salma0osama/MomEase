using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class SentMentalHealthTip
    {
        [Key]
        public int SentTipId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TipId { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("TipId")]
        public virtual MentalHealthTip Tip { get; set; }
    }
}
