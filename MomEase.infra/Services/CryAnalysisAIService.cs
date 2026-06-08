using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class CryAnalysisAIService : ICryAnalysisAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly ILogger<CryAnalysisAIService> _logger;

        public CryAnalysisAIService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<CryAnalysisAIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = config["AI:CryApiUrl"] ?? "https://ranaessayed-baby-cry-classifier.hf.space";
            _logger = logger;
        }

        public async Task<(CryReasonName prediction, double confidence, Dictionary<string, double> allScores)>
     AnalyzeAudioAsync(IFormFile audio)
        {
            try
            {
                _logger.LogInformation("🔍 Analyzing cry audio: {FileName}", audio.FileName);
                ValidateAudio(audio);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(3);

                // ✅ Step 1: Upload الملف
                using var uploadForm = new MultipartFormDataContent();
                var audioBytes = new byte[audio.Length];
                using (var ms = new MemoryStream())
                {
                    await audio.CopyToAsync(ms);
                    audioBytes = ms.ToArray();
                }

                // ✅ لو الملف 3gp، غيري اسمه لـ wav في الـ upload بس
                var uploadFileName = audio.FileName;
                var uploadMimeType = audio.ContentType ?? "audio/wav";

                var ext = Path.GetExtension(audio.FileName).ToLowerInvariant();
                if (ext == ".3gp")
                {
                    uploadFileName = Path.GetFileNameWithoutExtension(audio.FileName) + ".wav";
                    uploadMimeType = "audio/wav";
                }

                var byteContent = new ByteArrayContent(audioBytes);
                byteContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(uploadMimeType);
                uploadForm.Add(byteContent, "files", uploadFileName);

                var uploadUrl = $"{_apiUrl}/gradio_api/upload";
                _logger.LogInformation("📤 Uploading to {Url}", uploadUrl);

                var uploadResponse = await client.PostAsync(uploadUrl, uploadForm);
                var uploadText = await uploadResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("📥 Upload response: {R}", uploadText);

                if (!uploadResponse.IsSuccessStatusCode)
                    throw new Exception($"Upload failed: {uploadResponse.StatusCode} - {uploadText}");

                // بيرجع ["tmp/xxx.wav"]
                using var uploadDoc = JsonDocument.Parse(uploadText);
                var root = uploadDoc.RootElement;

                string uploadedPath;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    uploadedPath = root[0].GetString();
                }
                else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("files", out var filesArr))
                {
                    uploadedPath = filesArr[0].GetString();
                }
                else
                {
                    // log الشكل الفعلي عشان نشوف إيه اللي بيرجع
                    _logger.LogError("❌ Unexpected upload response structure: {Json}", uploadText);
                    throw new Exception($"Unexpected upload response: {uploadText}");
                }
                _logger.LogInformation("📁 Uploaded path: {P}", uploadedPath);
                _logger.LogInformation("📁 Uploaded path: {P}", uploadedPath);

                // ✅ Step 2: استدعاء الـ predict
                var predictBody = JsonSerializer.Serialize(new
                {
                    data = new[]
    {
        new
        {
            path = uploadedPath,
            orig_name = uploadFileName,        // ← كانت audio.FileName
            mime_type = uploadMimeType,         // ← كانت audio.ContentType ?? "audio/wav"
            meta = new { _type = "gradio.FileData" }
        }
    }
                });

                var predictContent = new StringContent(predictBody, Encoding.UTF8, "application/json");
                var predictUrl = $"{_apiUrl}/gradio_api/call/predict";
                _logger.LogInformation("📤 Calling predict at {Url}", predictUrl);

                var predictResponse = await client.PostAsync(predictUrl, predictContent);
                var predictText = await predictResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("📥 Predict response: {R}", predictText);

                if (!predictResponse.IsSuccessStatusCode)
                    throw new Exception($"Predict failed: {predictResponse.StatusCode} - {predictText}");

                // بيرجع { "event_id": "xxx" }
                using var eventDoc = JsonDocument.Parse(predictText);
                var eventId = eventDoc.RootElement.GetProperty("event_id").GetString();
                _logger.LogInformation("🎫 Event ID: {Id}", eventId);

                // ✅ Step 3: جيبي النتيجة
                var resultUrl = $"{_apiUrl}/gradio_api/call/predict/{eventId}";
                _logger.LogInformation("📤 Getting result from {Url}", resultUrl);

                var resultResponse = await client.GetAsync(resultUrl);
                var resultText = await resultResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("📥 Result: {R}", resultText);

                return ParseSSEResponse(resultText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cry analysis failed");
                throw new Exception($"Audio analysis failed: {ex.Message}", ex);
            }
        }

        private (CryReasonName prediction, double confidence, Dictionary<string, double> allScores)
            ParseSSEResponse(string sseText)
        {
            // SSE format: "data: [...]"
            var lines = sseText.Split('\n');
            string dataLine = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("data: "))
                {
                    dataLine = line.Substring(6).Trim();
                    if (dataLine != "[DONE]")
                        break;
                }
            }

            if (string.IsNullOrEmpty(dataLine))
                throw new Exception("No data in SSE response");

            // dataLine هيكون زي: [{"prediction": "laugh", ...}]
            using var doc = JsonDocument.Parse(dataLine);
            var arr = doc.RootElement;
            var resultElement = arr[0];

            string predictionStr;
            double confidence;
            var allScores = new Dictionary<string, double>();

            if (resultElement.ValueKind == JsonValueKind.String)
            {
                // لو رجع كـ string بـ single quotes
                var innerJson = resultElement.GetString() ?? "";
                if (innerJson.StartsWith("root="))
                    innerJson = innerJson.Substring(5);

                innerJson = innerJson
                    .Replace("'", "\"")
                    .Replace("True", "true")
                    .Replace("False", "false")
                    .Replace("None", "null");

                using var innerDoc = JsonDocument.Parse(innerJson);
                var inner = innerDoc.RootElement;
                predictionStr = inner.GetProperty("prediction").GetString() ?? "discomfort";
                confidence = inner.GetProperty("confidence").GetDouble();

                if (inner.TryGetProperty("all_scores", out var scoresEl))
                    foreach (var score in scoresEl.EnumerateObject())
                        allScores[score.Name] = score.Value.GetDouble();
            }
            else
            {
                predictionStr = resultElement.GetProperty("prediction").GetString() ?? "discomfort";
                confidence = resultElement.GetProperty("confidence").GetDouble();

                if (resultElement.TryGetProperty("all_scores", out var scoresEl))
                    foreach (var score in scoresEl.EnumerateObject())
                        allScores[score.Name] = score.Value.GetDouble();
            }

            _logger.LogInformation("🎯 Prediction: {P}, Confidence: {C}%", predictionStr, confidence);
            return (ParsePredictionToEnum(predictionStr), confidence, allScores);
        }

        private void ValidateAudio(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
                throw new ArgumentException("Audio file is empty");

            var ext = Path.GetExtension(audio.FileName).ToLowerInvariant();
            var allowed = new[] { ".wav", ".mp3", ".ogg", ".flac", ".m4a", ".webm", ".3gp" };
            if (!System.Array.Exists(allowed, e => e == ext))
                throw new ArgumentException($"Unsupported audio format: {ext}. Allowed: wav, mp3, ogg, flac, m4a, webm, 3gp");

            if (audio.Length > 20 * 1024 * 1024)
                throw new ArgumentException("Audio file is too large (maximum 20MB)");

            _logger.LogInformation("✅ Audio validation passed: {FileName} ({Size} KB)",
                audio.FileName, audio.Length / 1024);
        }

        private CryReasonName ParsePredictionToEnum(string prediction)
        {
            if (string.IsNullOrWhiteSpace(prediction))
                throw new ArgumentException("Prediction is empty");

            if (Enum.TryParse<CryReasonName>(prediction.Trim().ToLower(), true, out var result))
            {
                _logger.LogInformation("✅ Matched enum: {Enum}", result);
                return result;
            }

            // Manual mapping لو في اختلافات
            var mapping = new Dictionary<string, CryReasonName>(StringComparer.OrdinalIgnoreCase)
            {
                { "belly_pain", CryReasonName.belly_pain },
                { "bellypain",  CryReasonName.belly_pain },
                { "burping",    CryReasonName.burping },
                { "discomfort", CryReasonName.discomfort },
                { "hungry",     CryReasonName.hungry },
                { "laugh",      CryReasonName.laugh }
            };

            if (mapping.TryGetValue(prediction.Trim(), out var mapped))
            {
                _logger.LogInformation("✅ Mapped: '{Original}' → {Enum}", prediction, mapped);
                return mapped;
            }

            _logger.LogWarning("⚠️ Unknown prediction '{Prediction}', defaulting to discomfort", prediction);
            return CryReasonName.discomfort;
        }
    }
}