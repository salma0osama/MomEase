using PostCare.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class ChildVaccination
    {
        public int ChildVaccineId { get; set; }
        public int ChildId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? TakenDate { get; set; }
        public VaccineStatus Status { get; set; }

        // Navigation Properties
        public virtual Child Child { get; set; }
        public virtual Vaccinations Vaccination { get; set; }
    }
}
