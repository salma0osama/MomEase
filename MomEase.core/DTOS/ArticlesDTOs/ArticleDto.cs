using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class ArticleDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public int ReadingTimeMinutes { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string SourceUrl { get; set; }
        public string SourceName { get; set; }
        public bool IsSaved { get; set; }  // هل المستخدم حفظها؟
    }
}
