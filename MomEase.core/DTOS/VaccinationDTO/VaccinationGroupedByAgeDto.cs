using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.VaccinationDTO
{
    public class VaccinationGroupedByAgeDto
    {
        public int AgeInMonths { get; set; }
        public string AgeLabel { get; set; }   // "At Birth" / "2 Months" / etc.
        public DateTime ScheduledDate { get; set; }
        public List<ChildVaccinationDto> Vaccines { get; set; }
    }
}
