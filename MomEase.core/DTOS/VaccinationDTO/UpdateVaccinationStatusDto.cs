using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.VaccinationDTO
{
    public class UpdateVaccinationStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? TakenDate { get; set; }
    }
}