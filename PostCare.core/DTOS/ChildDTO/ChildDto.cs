using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.DTOS.ChildDTO
{
    public class ChildDto
    {
        public int ChildId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int AgeInMonths { get; set; }
        public int AgeInDays { get; set; }
        public string DeliveryType { get; set; }
        public string FeedingTypeForBaby { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
