using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Entities
{
    public class Vaccinations
    {
        public int ScheduleId { get; set; }
        public string Vaccine { get; set; }
        public int Age { get; set; }
        public string DoseTiming { get; set; }
        public string DiseasePrevented { get; set; }
        public string Dosage { get; set; }
        public string VaccinationWay { get; set; }

        // Navigation Properties
        public virtual ICollection<ChildVaccination> ChildVaccinations { get; set; }
    }
}
