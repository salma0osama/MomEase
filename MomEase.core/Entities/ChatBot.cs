using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ChatBot
    {
        [Key]
        public int ChatId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        public virtual ICollection<ChatMessages> ChatMessages { get; set; }
    }
}
