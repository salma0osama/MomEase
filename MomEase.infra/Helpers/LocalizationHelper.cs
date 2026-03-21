using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Helpers
{
    public static class LocalizationHelper
    {
        public static string GetLocalizedMessage(string key, string language, params object[] args)
        {
            var messages = language?.ToLower() == "ar"
                ? MessagesAr
                : MessagesEn;

            if (messages.TryGetValue(key, out var template))
            {
                return args.Length > 0
                    ? string.Format(template, args)
                    : template;
            }

            return key; // fallback
        }

        // ✅ الرسائل بالإنجليزي
        private static readonly Dictionary<string, string> MessagesEn = new()
        {
            // Sleep Messages
            { "NoSleepData", "No sleep data available for the specified period" },
            { "NoSleepHoursData", "No sleep hours data available" },
            { "SleepSignificantlyBelow", "⚠️⚠️ Average sleep ({0:F1} hours) is significantly below recommended ({1}-{2} hours). Please consult a pediatrician" },
            { "SleepSlightlyBelow", "⚠️ Average sleep ({0:F1} hours) is slightly below recommended ({1}-{2} hours)" },
            { "SleepNormal", "✅ Average sleep ({0:F1} hours) is within the normal range ({1}-{2} hours)" },
            { "SleepAboveAverage", "Average sleep ({0:F1} hours) is slightly above average ({1}-{2} hours), which is usually fine" },

            // Feeding Messages
            { "NoFeedingData", "No feeding data available for the specified period" },
            { "FeedingSignificantlyBelow", "⚠️⚠️ Average feedings ({0:F1} times/day) is significantly below recommended ({1}-{2} times). Please consult a pediatrician" },
            { "FeedingSlightlyBelow", "⚠️ Average feedings ({0:F1} times/day) is slightly below recommended ({1}-{2} times)" },
            { "FeedingNormal", "✅ Average feedings ({0:F1} times/day) is within the normal range ({1}-{2} times)" },
            { "FeedingAboveAverage", "Average feedings ({0:F1} times/day) is above average, which is usually good" },

            // Overall Status
            { "ExcellentNormalGrowth", "Excellent - Normal Growth" },
            { "NeedsAttention", "Needs Attention" },
            { "Good", "Good" },

            // Key Insights
            { "WeightUnderweight", "⚠️ Attention: Your child's weight is Underweight, follow-up with a doctor is recommended" },
            { "WeightOverweight", "⚠️ Attention: Your child's weight is Overweight, follow-up with a doctor is recommended" },
            { "SleepNeedsMore", "⚠️ Your child needs more sleep for better growth" },
            { "FeedingBelowRecommended", "⚠️ Number of feedings is below recommended" },
            { "SleepHelpsGrowth", "✅ Good sleep helps with better growth - keep it up!" },
            { "NormalGrowth", "✅ Your child is growing normally" },

            // Correlations
            { "NotEnoughDataSleepGrowth", "Not enough data to analyze the relationship between sleep and growth" },
            { "NotEnoughDataFeedingGrowth", "Not enough data to analyze the relationship between feeding and growth" },
            { "NotEnoughDataCorrelation", "Not enough data to analyze the relationship" },
            { "NoCorrelationObserved", "No clear relationship observed between sleep and growth in this period" },
            { "NoCorrelationObservedFeeding", "No clear relationship observed between feeding and growth in this period" },
            { "SleepGrowthPositive", "We noticed that during periods when your child slept well (12+ hours), growth was {0}% better" },
            { "FeedingGrowthPositive", "We noticed that days with regular feeding (5+ times) showed better growth" },

            // Overall Insights
            { "SleepFeedingTogether", "Good sleep and regular feeding together contribute to better growth" },
            { "SleepPositiveImpact", "Good sleep has a positive impact on your child's growth" },
            { "FeedingPositiveImpact", "Regular feeding has a positive impact on your child's growth" },
            { "ContinueRoutine", "Continue with current sleep and feeding routine" },

            // Recommendations
            { "IncreaseSleepHours", "🌙 Try to increase your child's sleep hours to the normal range" },
            { "MaintainSleepRoutine", "🌙 Maintain a regular sleep routine" },
            { "ContinueSleepRoutine", "✅ Continue with the current sleep routine - it's excellent" },
            { "IncreaseFeedings", "🍼 Try to increase the number of feedings" },
            { "EnsureAdequateFeeding", "🍼 Make sure the baby feeds adequately each time" },
            { "ContinueFeedingRoutine", "✅ Current feeding routine is excellent - keep it up" },
            { "ConsultPediatrician", "⚠️ Your child's weight is below normal - consult a pediatrician" },
            { "EnsureSufficientFeeding", "⚠️ Ensure adequate feeding" },
            { "ConsultDoctorOverweight", "⚠️ Your child's weight is above normal - consult a doctor" },
            { "GrowthNormalContinue", "✅ Your child's growth is normal - continue with the same routine" },
            { "SleepPriority", "💡 We noticed that good sleep helps your child's growth - make it a priority" },
            { "MonitorProgress", "📊 Monitor progress next month and compare with this report" }
        };

        // ✅ الرسائل بالعربي
        private static readonly Dictionary<string, string> MessagesAr = new()
        {
            // Sleep Messages
            { "NoSleepData", "لا توجد بيانات نوم متاحة للفترة المحددة" },
            { "NoSleepHoursData", "لا توجد بيانات ساعات نوم متاحة" },
            { "SleepSignificantlyBelow", "⚠️⚠️ متوسط النوم ({0:F1} ساعة) أقل بكثير من المطلوب ({1}-{2} ساعة). يُنصح باستشارة طبيب" },
            { "SleepSlightlyBelow", "⚠️ متوسط النوم ({0:F1} ساعة) أقل قليلاً من المطلوب ({1}-{2} ساعة)" },
            { "SleepNormal", "✅ متوسط النوم ({0:F1} ساعة) ضمن المدى الطبيعي ({1}-{2} ساعة)" },
            { "SleepAboveAverage", "متوسط النوم ({0:F1} ساعة) أعلى قليلاً من المتوسط ({1}-{2} ساعة)، وهذا عادةً جيد" },

            // Feeding Messages
            { "NoFeedingData", "لا توجد بيانات رضاعة متاحة للفترة المحددة" },
            { "FeedingSignificantlyBelow", "⚠️⚠️ متوسط الرضاعة ({0:F1} مرة/يوم) أقل بكثير من المطلوب ({1}-{2} مرات). استشيري طبيب" },
            { "FeedingSlightlyBelow", "⚠️ متوسط الرضاعة ({0:F1} مرة/يوم) أقل قليلاً من المطلوب ({1}-{2} مرات)" },
            { "FeedingNormal", "✅ متوسط الرضاعة ({0:F1} مرة/يوم) ضمن المدى الطبيعي ({1}-{2} مرات)" },
            { "FeedingAboveAverage", "متوسط الرضاعة ({0:F1} مرة/يوم) أعلى من المتوسط، وهذا عادةً جيد" },

            // Overall Status
            { "ExcellentNormalGrowth", "ممتاز - النمو طبيعي" },
            { "NeedsAttention", "يحتاج متابعة" },
            { "Good", "جيد" },

            // Key Insights
            { "WeightUnderweight", "⚠️ انتبهي: وزن طفلك أقل من الطبيعي، يُنصح بمتابعة مع الطبيب" },
            { "WeightOverweight", "⚠️ انتبهي: وزن طفلك أعلى من الطبيعي، يُنصح بمتابعة مع الطبيب" },
            { "SleepNeedsMore", "⚠️ طفلك يحتاج المزيد من النوم لنمو أفضل" },
            { "FeedingBelowRecommended", "⚠️ عدد الرضعات أقل من المطلوب" },
            { "SleepHelpsGrowth", "✅ النوم الجيد يساعد على نمو أفضل - استمري!" },
            { "NormalGrowth", "✅ طفلك ينمو بشكل طبيعي" },

            // Correlations
            { "NotEnoughDataSleepGrowth", "لا توجد بيانات كافية لتحليل العلاقة بين النوم والنمو" },
            { "NotEnoughDataFeedingGrowth", "لا توجد بيانات كافية لتحليل العلاقة بين الرضاعة والنمو" },
            { "NotEnoughDataCorrelation", "لا توجد بيانات كافية لتحليل العلاقة" },
            { "NoCorrelationObserved", "لم نلاحظ علاقة واضحة بين النوم والنمو في هذه الفترة" },
            { "NoCorrelationObservedFeeding", "لم نلاحظ علاقة واضحة بين الرضاعة والنمو في هذه الفترة" },
            { "SleepGrowthPositive", "✅ لاحظنا أنه في الفترات اللي طفلك نام فيها كويس (12+ ساعة)، النمو كان أفضل بنسبة {0}%" },
            { "FeedingGrowthPositive", "✅ لاحظنا أن الأيام اللي فيها رضاعة منتظمة (5+ مرات)، النمو كان أفضل" },

            // Overall Insights
            { "SleepFeedingTogether", "النوم الجيد والرضاعة المنتظمة معاً يساعدان على نمو أفضل" },
            { "SleepPositiveImpact", "النوم الجيد له تأثير إيجابي على نمو طفلك" },
            { "FeedingPositiveImpact", "الرضاعة المنتظمة لها تأثير إيجابي على نمو طفلك" },
            { "ContinueRoutine", "استمري على نظام النوم والرضاعة الحالي" },

            // Recommendations
            { "IncreaseSleepHours", "🌙 حاولي زيادة ساعات نوم طفلك إلى المدى الطبيعي" },
            { "MaintainSleepRoutine", "🌙 حافظي على روتين نوم منتظم" },
            { "ContinueSleepRoutine", "✅ استمري على نظام النوم الحالي - إنه ممتاز" },
            { "IncreaseFeedings", "🍼 حاولي زيادة عدد الرضعات" },
            { "EnsureAdequateFeeding", "🍼 تأكدي أن الطفل يرضع بشكل كافٍ في كل مرة" },
            { "ContinueFeedingRoutine", "✅ نظام الرضاعة الحالي ممتاز - استمري عليه" },
            { "ConsultPediatrician", "⚠️ وزن طفلك أقل من الطبيعي - استشيري طبيب أطفال" },
            { "EnsureSufficientFeeding", "⚠️ تأكدي من كفاية الرضاعة" },
            { "ConsultDoctorOverweight", "⚠️ وزن طفلك أعلى من الطبيعي - استشيري طبيب" },
            { "GrowthNormalContinue", "✅ نمو طفلك طبيعي - استمري على نفس النظام" },
            { "SleepPriority", "💡 لاحظنا أن نوم طفلك الجيد يساعد على نموه - اجعليه أولوية" },
            { "MonitorProgress", "📊 راقبي التطور في الشهر القادم وقارنيه بهذا التقرير" }
        };
    }
}
