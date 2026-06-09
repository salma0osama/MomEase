using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class SearchHistory
    {
        [Key]
        public int SearchId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SearchTerm { get; set; }

        [Required]
        public DateTime SearchedAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }
    }
}
