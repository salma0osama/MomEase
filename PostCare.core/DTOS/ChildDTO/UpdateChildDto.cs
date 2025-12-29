using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.ChildDTO
{
    public class UpdateChildDto
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? DeliveryType { get; set; }

        public string? FeedingTypeForBaby { get; set; }
    }
}
