using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class SearchResultDto
    {
        public IEnumerable<ArticleListDto> Articles { get; set; }
        public IEnumerable<CategoryResultDto> Categories { get; set; }
    }

    /// <summary>
    /// بيانات الفئة في نتيجة البحث
    /// </summary>
    public class CategoryResultDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int ArticlesCount { get; set; }  // عدد المقالات في هذه الفئة
    }
}

