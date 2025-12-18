using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ChatBot
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime Created_At { get; set; }


        // Navigation Properties
        public virtual Users User { get; set; }
        public virtual ICollection<ChatMessages> ChatMessages { get; set; }
    }
}
