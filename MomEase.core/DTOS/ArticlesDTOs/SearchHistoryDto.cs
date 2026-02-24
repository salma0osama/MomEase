using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class SearchHistoryDto
    {
        public int SearchId { get; set; }
        public string SearchTerm { get; set; }
        public DateTime SearchedAt { get; set; }
    }
}
