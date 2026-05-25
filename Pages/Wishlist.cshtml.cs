using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages
{
    [Authorize(Roles = "Customer")]
    public class WishlistModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WishlistModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<NearGo.Models.Supermarket> FollowedSupermarkets { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            FollowedSupermarkets = await _context.Supermarkets
                .Where(s => s.Followers.Any(f => f.Id == user.Id))
                .OrderByDescending(s => s.Rating)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleFollowAsync(int supermarketId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var supermarket = await _context.Supermarkets
                .Include(s => s.Followers)
                .FirstOrDefaultAsync(s => s.Id == supermarketId);
            if (supermarket == null) return NotFound();

            var follower = supermarket.Followers.FirstOrDefault(f => f.Id == user.Id);
            if (follower != null)
            {
                supermarket.Followers.Remove(follower);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
