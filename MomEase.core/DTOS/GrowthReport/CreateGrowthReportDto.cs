using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class CreateGrowthReportDto
    {
        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        // Optional: إذا كانت الأم عايزة تحدد بالشهور
        public int? LastMonths { get; set; }
    }
}
