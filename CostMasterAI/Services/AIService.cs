using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CostMasterAI.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;

        // API Key Google Gemini lo
        private const string ApiKey = "AIzaSyC-moDDIsYHjDx5jFLoNbJALhl-LmBCvsc";

        // Base Endpoint Gemini
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AIService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateMarketingCopyAsync(string recipeName, string ingredients)
        {
            // --- MODE DEMO (Jaga-jaga kalau key kosong) ---
            if (string.IsNullOrEmpty(ApiKey))
            {
                await Task.Delay(2000);
                return $"[AI DEMO]: Wuih! {recipeName} ini rasanya pecah banget! Dibuat dari {ingredients} pilihan terbaik. Order sekarang bro!";
            }

            // --- MODE GEMINI REAL ---
            try
            {
                var prompt = $"Buatkan deskripsi makanan yang menggugah selera untuk menu '{recipeName}'. Bahannya: {ingredients}. Gaya bahasa: Gaul, santai, tapi menjual (copywriting). Maksimal 50 kata.";

                // 1. Struktur Request KHUSUS GEMINI (Beda sama OpenAI)
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 2. URL Request (API Key ditaruh di Query Param kalau Gemini)
                var requestUrl = $"{BaseUrl}?key={ApiKey}";

                // 3. Tembak API
                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var node = JsonNode.Parse(responseString);

                    // 4. Cara Baca Response GEMINI
                    // Path: candidates[0] -> content -> parts[0] -> text
                    var resultText = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    return resultText ?? "AI Bingung (Output Kosong).";
                }

                return $"Error Gemini: {response.StatusCode} - {responseString}";
            }
            catch (Exception ex)
            {
                return $"Gagal connect ke AI: {ex.Message}";
            }
        }
    }
}