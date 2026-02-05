using MomEase.core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class ChildVaccination
    {
        [Key]
        public int ChildVaccineId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? TakenDate { get; set; }

        [Required]
        public VaccineStatus Status { get; set; } = VaccineStatus.No;

        // Navigation Properties
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Vaccinations Vaccination { get; set; }
    }
}
