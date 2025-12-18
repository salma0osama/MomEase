using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class MotherProfile
    {
        public int MotherId { get; set; }
        public int UserId { get; set; }
        public bool IsFirstTimeMother { get; set; }
        public int NumberOfChildren { get; set; }
        public MentalHealthStatus? MentalHealthStatus { get; set; }
        public HealthStatus? HealthStatus { get; set; }

        // Navigation Properties
        public virtual Users User { get; set; }
    }
}
