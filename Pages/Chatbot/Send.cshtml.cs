using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Data;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Chatbot
{
    public class SendModel : PageModel
    {
        private readonly OpenAIService _openAIService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SendModel(OpenAIService openAIService, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _openAIService = openAIService;
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _openAIService.GetChatResponse(Message);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User)!;
                _context.ChatMessages.Add(new NearGo.Models.ChatMessage
                {
                    UserId = userId,
                    UserMessage = Message,
                    AiResponse = response,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { response });
        }
    }
}
