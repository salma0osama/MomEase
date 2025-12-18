using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Articles
    {
        public int ArticleId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string SourceUrl { get; set; }
        public string SourceName { get; set; }
        public DateTime? PublishedDate { get; set; }

        // Navigation Properties
        public virtual ArticleCategories Category { get; set; }
    }
}
