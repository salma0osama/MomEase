using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ChatBot
{
    public class MessageDto
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
