using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.CommunityDTO
{
    public class SavedPostDto
    {
        public int SavedPostId { get; set; }
        public int PostId { get; set; }
        public DateTime SavedAt { get; set; }
        public CommunityPostDto Post { get; set; } = null!;
    }
}