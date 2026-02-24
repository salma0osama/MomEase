using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class SaveArticleRequestDto
    {
        [Required(ErrorMessage = "Article ID is required")]
        public int ArticleId { get; set; }
    }
}
