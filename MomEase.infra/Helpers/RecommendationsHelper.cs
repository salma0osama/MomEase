using System.Collections.Generic;

namespace MomEase.infra.Helpers
{
    public static class RecommendationsHelper
    {
        public static List<string> GetRecommendations(
            string severityLevel,
            string language)
        {
            bool isArabic = language.StartsWith("ar");
            severityLevel = NormalizeLevel(severityLevel);

            if (isArabic)
            {
                return severityLevel switch
                {
                    "Minimal" => new List<string>
                    {
                        "خذي وقتاً لنفسك يومياً حتى لو دقائق بسيطة.",
                        "حافظي على التواصل مع العائلة أو الأصدقاء.",
                        "استمري في العادات الصحية مثل النوم الجيد.",
                        "تذكري أن الاهتمام بنفسك مهم دائماً."
                    },

                    "Mild" => new List<string>
                    {
                        "تحدثي مع شخص تثقين به عن مشاعرك.",
                        "مارسي أنشطة بسيطة تساعدك على الاسترخاء.",
                        "حاولي تنظيم يومك وتقليل التوتر.",
                        "تابعي حالتك وإذا زادت الأعراض استشيري مختص."
                    },

                    "Moderate" => new List<string>
                    {
                        "يفضل استشارة مختص نفسي في أقرب وقت.",
                        "لا تبقي وحدك واطلبي الدعم من المقربين.",
                        "جربي أنشطة خفيفة مثل المشي أو الكتابة.",
                        "حاولي الالتزام بروتين يومي منتظم."
                    },

                    "Severe" => new List<string>
                    {
                        "تواصلي مع مختص نفسي أو طبيب فوراً.",
                        "لا تترددي في طلب المساعدة من أي شخص قريب.",
                        "إذا شعرتِ بخطر، تواصلي مع جهة دعم فوراً.",
                        "تذكري أنكِ لستِ وحدك وهناك من يمكنه مساعدتك."
                    },

                    _ => new List<string>()
                };
            }
            else
            {
                return severityLevel switch
                {
                    "Minimal" => new List<string>
                    {
                        "Take small moments for yourself every day.",
                        "Stay connected with friends and family.",
                        "Maintain healthy habits like good sleep.",
                        "Remember that self-care is always important."
                    },

                    "Mild" => new List<string>
                    {
                        "Talk to someone you trust about your feelings.",
                        "Practice simple relaxing activities.",
                        "Try to organize your day and reduce stress.",
                        "Monitor your condition and seek help if it worsens."
                    },

                    "Moderate" => new List<string>
                    {
                        "Consider consulting a mental health professional.",
                        "Do not isolate yourself—seek support from others.",
                        "Try light activities like walking or journaling.",
                        "Stick to a consistent daily routine."
                    },

                    "Severe" => new List<string>
                    {
                        "Seek professional help immediately.",
                        "Reach out to someone you trust right away.",
                        "If you feel at risk, contact emergency support.",
                        "Remember you are not alone and help is available."
                    },

                    _ => new List<string>()
                };
            }
        }

        private static string NormalizeLevel(string level)
        {
            if (string.IsNullOrEmpty(level))
                return "Minimal";

            level = level.ToLower();

            if (level.Contains("شديد"))
                return "Severe";

            if (level.Contains("متوسط"))
                return "Moderate";

            if (level.Contains("خفيف") || level.Contains("بسيط"))
                return "Mild";

            return "Minimal";
        }

    }
}