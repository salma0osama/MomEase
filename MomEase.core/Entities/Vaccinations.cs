using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.Entities
{
    public class Vaccinations
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Vaccine { get; set; }

        [Required]
        public int Age { get; set; }

        [MaxLength(100)]
        public string DoseTiming { get; set; }

        [MaxLength(255)]
        public string DiseasePrevented { get; set; }

        [MaxLength(100)]
        public string Dosage { get; set; }

        [MaxLength(100)]
        public string VaccinationWay { get; set; }

        // Navigation Property
        public virtual ICollection<ChildVaccination> ChildVaccinations { get; set; }
    }
}
