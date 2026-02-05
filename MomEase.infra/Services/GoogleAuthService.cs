using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                // ✅ تأكد من إن الـ Token مش null أو فاضي
                if (string.IsNullOrWhiteSpace(idToken))
                {
                    throw new Exception("ID Token is null or empty");
                }

                // ✅ جيب الـ Client ID من الـ Configuration
                var clientId = _configuration["Authentication:Google:ClientId"];

                // ✅ تأكد إن الـ Client ID موجود
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new Exception("Google Client ID is not configured in appsettings.json");
                }

                Console.WriteLine($"🔑 Validating token with Client ID: {clientId}");

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                // ✅ Validate الـ Token
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                // ✅ تأكد إن الـ payload مش null
                if (payload == null)
                {
                    throw new Exception("Token validation returned null payload");
                }

                Console.WriteLine($"✅ Token validated successfully for: {payload.Email}");

                return payload;
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine($"❌ Invalid JWT: {ex.Message}");
                throw new Exception($"Invalid Google token: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Validation error: {ex.Message}");
                throw new Exception($"Token validation failed: {ex.Message}");
            }
        }
    }
}
