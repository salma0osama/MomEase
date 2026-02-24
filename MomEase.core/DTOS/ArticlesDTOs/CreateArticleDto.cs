using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class CreateArticleDto
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(500)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid image URL")]
        public string ImageUrl { get; set; }

        [Url(ErrorMessage = "Invalid source URL")]
        public string SourceUrl { get; set; }

        [MaxLength(255)]
        public string SourceName { get; set; }
    }
}
