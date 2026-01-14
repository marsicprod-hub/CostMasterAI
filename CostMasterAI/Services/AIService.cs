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

        // GANTI API KEY LO DISINI
        private const string ApiKey = "AIzaSyC-moDDIsYHjDx5jFLoNbJALhl-LmBCvsc";
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public AIService()
        {
            _httpClient = new HttpClient();
        }

        // --- FITUR LAMA (Marketing) ---
        public async Task<string> GenerateMarketingCopyAsync(string recipeName, string ingredients)
        {
            var prompt = $"Buatkan deskripsi makanan yang menggugah selera untuk menu '{recipeName}'. Bahannya: {ingredients}. Gaya bahasa: Gaul, santai, tapi menjual (copywriting). Maksimal 50 kata.";
            return await SendPromptAsync(prompt);
        }

        // --- FITUR BARU (Recipe Generator - JSON) ---
        public async Task<string> GenerateRecipeDataAsync(string recipeName)
        {
            // Prompt Canggih: Minta output JSON murni
            var prompt = $@"
                Bertindaklah sebagai Chef dan Konsultan Bisnis Kuliner.
                Saya ingin membuat menu: '{recipeName}'.
                
                Tugasmu:
                1. Tentukan bahan-bahan bakunya.
                2. Tentukan estimasi jumlah pemakaian untuk 1 porsi.
                3. Tentukan estimasi HARGA PASARAN (IDR) bahan tersebut per kemasan umum.
                
                OUTPUT WAJIB FORMAT JSON (Tanpa markdown, tanpa teks lain, langsung kurung siku array):
                [
                    {{
                        ""ingredient_name"": ""Nama Bahan (Contoh: Tepung Terigu)"",
                        ""usage_qty"": 100,
                        ""usage_unit"": ""Gram"",
                        ""estimated_price_per_package"": 15000,
                        ""package_qty"": 1000,
                        ""package_unit"": ""Gram""
                    }}
                ]
                
                Pastikan unit pemakaian dan unit beli (package) kompatibel atau masuk akal.
            ";

            return await SendPromptAsync(prompt);
        }

        // --- HELPER KIRIM REQUEST ---
        private async Task<string> SendPromptAsync(string prompt)
        {
            if (string.IsNullOrEmpty(ApiKey)) return "API Key Missing";

            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var requestUrl = $"{BaseUrl}?key={ApiKey}";

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var node = JsonNode.Parse(responseString);
                    var result = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    // Bersihin Markdown Code Block kalau si AI bandel ngasih ```json
                    if (result != null)
                    {
                        result = result.Replace("```json", "").Replace("```", "").Trim();
                    }
                    return result ?? "";
                }

                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}