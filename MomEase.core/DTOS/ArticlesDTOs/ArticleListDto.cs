using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class ArticleListDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string ShortDescription { get; set; }  // أول 150 حرف
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public int ReadingTimeMinutes { get; set; }
        public bool IsSaved { get; set; }
    }
}
