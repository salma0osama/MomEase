using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Helpers
{
    public static class LanguageHelper
    {
        /// <summary>
        /// يرجع النص العربي أو الإنجليزي حسب اللغة المطلوبة
        /// </summary>
        /// <param name="arValue">النص العربي (nullable)</param>
        /// <param name="enValue">النص الإنجليزي (required)</param>
        /// <param name="lang">اللغة المطلوبة (ar أو en)</param>
        /// <returns>النص المناسب حسب اللغة</returns>
        public static string GetLocalized(
            string? arValue,
            string enValue,
            string lang)
        {
            // لو اللغة عربي والنص العربي موجود، ارجع العربي
            // لو مش موجود أو اللغة إنجليزي، ارجع الإنجليزي
            return lang.StartsWith("ar") && !string.IsNullOrEmpty(arValue)
                ? arValue
                : enValue;
        }

        /// <summary>
        /// يجيب اللغة من الـ HTTP Request Header
        /// </summary>
        /// <param name="httpContextAccessor">للوصول للـ HTTP Context</param>
        /// <returns>"ar" أو "en" (default: "en")</returns>
        public static string GetLang(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?
                .Request.Headers["Accept-Language"]
                .ToString().ToLower() ?? "en";
        }
    }
}
