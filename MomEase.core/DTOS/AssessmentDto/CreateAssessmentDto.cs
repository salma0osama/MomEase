using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.AssessmentDto
{
    public class CreateAssessmentDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string? NameAr { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        [Required]
        [PositiveNumber]
        public int TotalQuestions { get; set; }

        [Required]
        [PositiveNumber]
        public int MaxScore { get; set; }
    }
    public class PositiveNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && int.TryParse(value.ToString(), out int result))
            {
                if (result <= 0)
                {
                    return new ValidationResult("Value must be grater than zero");
                }
            }
            return ValidationResult.Success;
        }
    }
}
