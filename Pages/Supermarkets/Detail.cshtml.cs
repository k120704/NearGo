using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.Supermarkets
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context) => _context = context;

        public NearGo.Models.Supermarket? Supermarket { get; set; }
        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            Supermarket = await _context.Supermarkets.FirstOrDefaultAsync(s => s.Slug == slug);
            if (Supermarket == null) return NotFound();

            var now = DateTime.UtcNow;
            Products = await _context.Products
                .Where(p => p.SupermarketId == Supermarket.Id && p.IsActive && p.StockQuantity > 0 && p.ExpiryDate > now)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
