using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.VaccinationDTO
{
    public class ChildVaccinationDto
    {
        public int ChildVaccineId { get; set; }
        public int ChildId { get; set; }
        public int ScheduleId { get; set; }
        public string VaccineName { get; set; }
        public string DoseTiming { get; set; }
        public string DiseasePrevented { get; set; }
        public string Dosage { get; set; }
        public string VaccinationWay { get; set; }
        public int AgeInMonths { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? TakenDate { get; set; }
        public string Status { get; set; }
    }
}
