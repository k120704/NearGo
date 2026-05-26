using System.Text;
using Microsoft.Extensions.Options;
using NearGo.Configurations;
using Newtonsoft.Json;

namespace NearGo.Services
{
    public class OpenAIService
    {
        private readonly OpenAISettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IOptions<OpenAISettings> settings, HttpClient httpClient, ILogger<OpenAIService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetChatResponse(string userMessage, List<ChatHistoryItem>? history = null)
        {
            try
            {
                var messages = new List<object>
                {
                    new
                    {
                        role = "system",
                        content = "Bạn là trợ lý AI của NearGo - nền tảng thương mại điện tử bán sản phẩm cận date dành cho siêu thị. Bạn giúp khách hàng tìm kiếm sản phẩm, giải đáp thắc mắc về sản phẩm cận date, gợi ý mua sắm tiết kiệm. Hãy trả lời bằng tiếng Việt, thân thiện, ngắn gọn."
                    }
                };

                if (history != null)
                {
                    foreach (var item in history.TakeLast(10))
                    {
                        messages.Add(new { role = "user", content = item.UserMessage });
                        messages.Add(new { role = "assistant", content = item.AiResponse });
                    }
                }

                messages.Add(new { role = "user", content = userMessage });

                var payload = new
                {
                    model = _settings.Model,
                    messages,
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Response}", response.StatusCode, responseJson);
                    return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                if (result?.choices != null && result.choices[0]?.message?.content != null)
                {
                    return result.choices[0].message.content.ToString();
                }

                _logger.LogWarning("OpenAI returned unexpected format: {Response}", responseJson);
                return "Xin lỗi, tôi không thể xử lý yêu cầu này ngay bây giờ.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI service exception");
                return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
            }
        }
    }

    public class ChatHistoryItem
    {
        public string UserMessage { get; set; } = string.Empty;
        public string AiResponse { get; set; } = string.Empty;
    }
}
