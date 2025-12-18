using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ChatMessages
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual ChatBot Chat { get; set; }
    }
}

