using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticleCategories
{
    public class UpdateArticleCategoryDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        [AllowEmptyUrl(ErrorMessage = "Please provide a valid URL or leave it empty")]
        public string? ImageUrl { get; set; }
    }
}
