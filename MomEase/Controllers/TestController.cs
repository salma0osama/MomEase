using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using MomEase.infra.Helpers;

namespace MomEase.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Test Language Detection
        /// </summary>
        [HttpGet("language")]
        public IActionResult TestLanguage()
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);

            return Ok(new
            {
                DetectedLanguage = lang,
                Message = lang == "ar" ? "اللغة المكتشفة: عربي" : "Detected Language: English",
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                QueryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString())
            });
        }

        /// <summary>
        /// Test Localized Text
        /// </summary>
        [HttpGet("localized-text")]
        public IActionResult TestLocalizedText()
        {
            var lang = LanguageHelper.GetLang(_httpContextAccessor);

            var greeting = LanguageHelper.GetLocalized(
                arValue: "مرحباً بك في MomEase",
                enValue: "Welcome to MomEase",
                lang: lang
            );

            return Ok(new
            {
                Language = lang,
                Greeting = greeting
            });
        }
    }
}
