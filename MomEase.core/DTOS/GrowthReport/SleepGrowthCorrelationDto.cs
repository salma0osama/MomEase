using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class SleepGrowthCorrelationDto
    {
        public bool HasCorrelation { get; set; }
        public string Type { get; set; } // "Positive" / "Negative" / "None"
        public string Message { get; set; }
        // مثال: "لاحظنا أنه في الشهور اللي الطفل نام فيها كويس، النمو كان أفضل"
    }
}
