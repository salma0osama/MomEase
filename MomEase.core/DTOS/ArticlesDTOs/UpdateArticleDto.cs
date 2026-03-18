using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticlesDTOs
{
    public class UpdateArticleDto
    {
        public int? CategoryId { get; set; }

        [MaxLength(500)]
        public string? Title { get; set; }

        public string? Content { get; set; }
        // ⭐ Arabic (Optional)
        [MaxLength(500)]
        public string? TitleAr { get; set; }

        public string? ContentAr { get; set; }

        [UrlOrEmpty]
        public string? ImageUrl { get; set; }

        [UrlOrEmpty]
        public string? SourceUrl { get; set; }

        [MaxLength(255)]
        public string? SourceName { get; set; }
    }

    // Custom Attribute
    public class UrlOrEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            var url = value.ToString();
            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("The field must be a valid URL or empty.");
        }
    }
}
