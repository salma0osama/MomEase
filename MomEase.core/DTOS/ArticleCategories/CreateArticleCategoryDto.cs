using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticleCategories
{
    public class CreateArticleCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        // ⭐ Arabic (Optional)
        [MaxLength(255)]
        public string? NameAr { get; set; }

        public string? DescriptionAr { get; set; }
        [Required(ErrorMessage = "Category image is required")]
        [Url(ErrorMessage = "Invalid image URL")]
        public string ImageUrl { get; set; }

    }
}
