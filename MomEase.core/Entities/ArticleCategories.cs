using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ArticleCategories
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        // ⭐ Arabic (Optional - Nullable)
        [MaxLength(255)]
        public string? NameAr { get; set; }

        public string? DescriptionAr { get; set; }
        // Navigation Property
        public virtual ICollection<Articles> Articles { get; set; }
    }
}
