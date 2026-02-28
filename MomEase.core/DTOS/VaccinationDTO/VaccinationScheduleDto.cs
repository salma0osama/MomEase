using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.VaccinationDTO
{
    public class VaccinationScheduleDto
    {
        public int ScheduleId { get; set; }
        public string Vaccine { get; set; }
        public int AgeInMonths { get; set; }
        public string DoseTiming { get; set; }
        public string DiseasePrevented { get; set; }
        public string Dosage { get; set; }
        public string VaccinationWay { get; set; }
    }
}
