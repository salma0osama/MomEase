using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ChatBot
{
    public class ChatHistoryDto
    {
        public int ChatId { get; set; }
        public List<MessageDto> Messages { get; set; }
    }
}
