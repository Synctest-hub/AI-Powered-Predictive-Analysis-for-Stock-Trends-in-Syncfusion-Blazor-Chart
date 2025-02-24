using System.Text;
using System.Text.Json;
using static AIStockTrends.Components.Pages.Home;

namespace AIStockTrends
{
    public class AzureOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint = "{YOUR_END_POINT}";
        private readonly string _apiKey = "{YOUR_API_KEY}";
        private readonly string _deployment = "{YOUR_MODEL_NAME";

        public AzureOpenAIService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetResponseFromOpenAI(string prompt)
        {
            var requestBody = new
            {
                messages = new[]
            {
               new { role = "user", content = prompt }
            },
                max_tokens = 2000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/openai/deployments/{_deployment}/chat/completions?api-version=2023-07-01-preview")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorContent}";
            }
        }

        public string GeneratePrompt(List<StockDataModel> historicalData)
        {
            var prompt = new StringBuilder("Predict OHLC values for the next 45 days based on the given historical data:\n");

            foreach (var data in historicalData)
            {
                prompt.AppendLine($"{data.Period:yyyy-MM-dd}: {data.High}, {data.Low}, {data.Open}, {data.Close}");
            }

            prompt.AppendLine("\n### STRICT OUTPUT REQUIREMENTS ###");
            prompt.AppendLine("- Generate EXACTLY 45 rows of data.");
            prompt.AppendLine("- Each row must be in the format: yyyy-MM-dd:High:Low:Open:Close.");
            prompt.AppendLine("- The predictions must include a natural mix of both upward and downward trends.");
            prompt.AppendLine("- NO missing or duplicate dates.");
            prompt.AppendLine("- NO extra text, explanations, or labels—just raw data.");
            prompt.AppendLine("- Ensure that each day's values are **realistic** and follow stock market behavior.");

            return prompt.ToString();
        }
    }
}
