using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class SavedArticles
    {
        [Key]
        public int SavedArticleId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Articles Article { get; set; }
    }
}
