using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.ArticleCategories
{
    public class AllowEmptyUrlAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var strValue = value as string;

            // 1. لو القيمة null أو فاضية أو كلمة string، هنعتبرها صالحة (عشان الـ Service يتجاهلها)
            if (string.IsNullOrWhiteSpace(strValue) || strValue == "string")
            {
                return ValidationResult.Success;
            }

            // 2. لو فيها نص، نتأكد إنه URL فعلاً
            bool isUrl = Uri.TryCreate(strValue, UriKind.Absolute, out var uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (isUrl)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "Invalid image URL format");
        }
    }
}
