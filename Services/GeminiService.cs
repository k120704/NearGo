using System.Text;
using Microsoft.Extensions.Options;
using NearGo.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NearGo.Services
{
    public class GeminiService
    {
        private readonly GeminiSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IOptions<GeminiSettings> settings, HttpClient httpClient, ILogger<GeminiService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetChatResponse(string userMessage, List<ChatHistoryItem>? history = null, string? dataContext = null)
        {
            try
            {
                var systemPrompt = "Bạn là trợ lý AI của NearGo - nền tảng thương mại điện tử bán sản phẩm cận date dành cho siêu thị. Bạn giúp khách hàng tìm kiếm sản phẩm, giải đáp thắc mắc về sản phẩm cận date, gợi ý mua sắm tiết kiệm.\n\nQUY TẮC TRẢ LỜI:\n- Chỉ trả lời bằng văn bản thuần, KHÔNG dùng markdown (không *, không -, không bullet list)\n- KHÔNG đọc lại hay copy dữ liệu sản phẩm từ phần [DỮ LIỆU...] vào câu trả lời\n- KHÔNG thêm link hay URL\n- Nói tự nhiên như người bán hàng ngoài đời thực\n- Sản phẩm sẽ tự động hiện thẻ bên dưới tin nhắn, bạn chỉ cần nói tên và mô tả ngắn\n- Trả lời ngắn gọn, tối đa 2-3 câu";

                if (!string.IsNullOrEmpty(dataContext))
                {
                    systemPrompt += "\n\n" + dataContext;
                }

                var contents = new JArray
                {
                    new JObject
                    {
                        ["role"] = "user",
                        ["parts"] = new JArray { new JObject { ["text"] = systemPrompt } }
                    },
                    new JObject
                    {
                        ["role"] = "model",
                        ["parts"] = new JArray { new JObject { ["text"] = "Tôi hiểu. Tôi sẽ hỗ trợ bạn!" } }
                    }
                };

                if (history != null)
                {
                    foreach (var item in history.TakeLast(10))
                    {
                        contents.Add(new JObject
                        {
                            ["role"] = "user",
                            ["parts"] = new JArray { new JObject { ["text"] = item.UserMessage } }
                        });
                        contents.Add(new JObject
                        {
                            ["role"] = "model",
                            ["parts"] = new JArray { new JObject { ["text"] = item.AiResponse } }
                        });
                    }
                }

                contents.Add(new JObject
                {
                    ["role"] = "user",
                    ["parts"] = new JArray { new JObject { ["text"] = userMessage } }
                });

                var requestBody = new JObject
                {
                    ["contents"] = contents,
                    ["generationConfig"] = new JObject
                    {
                        ["maxOutputTokens"] = 1000,
                        ["temperature"] = 0.7
                    }
                };

                var json = requestBody.ToString();
                var url = $"https://generativelanguage.googleapis.com/v1/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseJson);
                    return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                if (result?.candidates != null && result.candidates[0]?.content?.parts != null && result.candidates[0].content.parts[0]?.text != null)
                {
                    return result.candidates[0].content.parts[0].text.ToString();
                }

                _logger.LogWarning("Gemini returned unexpected format: {Response}", responseJson);
                return "Xin lỗi, tôi không thể xử lý yêu cầu này ngay bây giờ.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini service exception");
                return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
            }
        }
    }
}
