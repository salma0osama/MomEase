using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.core.DTOS.GrowthReport
{
    public class GrowthReportContentDto
    {
        // ═══════════════════════════════════════
        // 📊 ملخص عام
        // ═══════════════════════════════════════
        public ReportSummaryDto Summary { get; set; }

        // ═══════════════════════════════════════
        // 📈 بيانات النمو
        // ═══════════════════════════════════════
        public GrowthAnalysisDto GrowthAnalysis { get; set; }

        // ═══════════════════════════════════════
        // 😴 بيانات النوم
        // ═══════════════════════════════════════
        public SleepAnalysisDto SleepAnalysis { get; set; }

        // ═══════════════════════════════════════
        // 🍼 بيانات الأكل
        // ═══════════════════════════════════════
        public FeedingAnalysisDto FeedingAnalysis { get; set; }

        // ═══════════════════════════════════════
        // 🔗 العلاقة بين البيانات
        // ═══════════════════════════════════════
        public CorrelationAnalysisDto Correlations { get; set; }

        // ═══════════════════════════════════════
        // 📊 بيانات الرسومات
        // ═══════════════════════════════════════
        public ReportChartsDto Charts { get; set; }

        // ═══════════════════════════════════════
        // 💡 التوصيات
        // ═══════════════════════════════════════
        public List<string> Recommendations { get; set; }
    }
}
