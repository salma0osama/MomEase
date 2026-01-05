using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.ChildDTO
{
    public class CreateChildDto
    {
        [Required(ErrorMessage = "اسم الطفل مطلوب")]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "جنس الطفل مطلوب")]
        public string Gender { get; set; } // "Male" or "Female"

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "نوع الولادة مطلوب")]
        public string DeliveryType { get; set; } // "Natural" or "Cesarean"

        public string FeedingTypeForBaby { get; set; } // "Breastfeeding", "Formula", "Mixed"
    }

}
