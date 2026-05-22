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
    [IgnoreAntiforgeryToken]
    public class WishlistModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WishlistModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Product> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            Products = await _context.Wishlists
                .Include(w => w.Product).ThenInclude(p => p.Supermarket)
                .Where(w => w.UserId == userId && w.Product.IsActive)
                .Select(w => w.Product)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleAsync(int productId)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                await _context.SaveChangesAsync();
                return new JsonResult(new { liked = false });
            }

            _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
            await _context.SaveChangesAsync();
            return new JsonResult(new { liked = true });
        }
    }
}
