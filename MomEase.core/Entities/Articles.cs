using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Articles
    {
        [Key]
        public int ArticleId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string SourceUrl { get; set; }

        [MaxLength(255)]
        public string SourceName { get; set; }

        public DateTime? PublishedDate { get; set; }

        // Navigation Property
        [ForeignKey("CategoryId")]
        public virtual ArticleCategories Category { get; set; }
        public virtual ICollection<SavedArticles> SavedByUsers { get; set; }

    }
}
