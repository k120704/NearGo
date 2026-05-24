using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Data;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Chatbot
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    [IgnoreAntiforgeryToken]
    public class SendModel : PageModel
    {
        private readonly GeminiService _geminiService;
        private readonly ChatbotContextService _contextService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SendModel(GeminiService geminiService, ChatbotContextService contextService, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _geminiService = geminiService;
            _contextService = contextService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync([FromBody] ChatRequest request)
        {
            var ctx = await _contextService.BuildContext(request.Message, User);
            var response = await _geminiService.GetChatResponse(request.Message, null, ctx.ContextText);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User)!;
                _context.ChatMessages.Add(new NearGo.Models.ChatMessage
                {
                    UserId = userId,
                    UserMessage = request.Message,
                    AiResponse = response,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { response, products = ctx.Products, supermarkets = ctx.Supermarkets });
        }
    }
}
