using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ChatMessages
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ChatId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Sender { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);

        // Navigation Property
        [ForeignKey("ChatId")]
        public virtual ChatBot Chat { get; set; }
    }
}

