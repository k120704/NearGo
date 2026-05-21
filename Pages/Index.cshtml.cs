using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();
        public List<Banner> Banners { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<FlashSaleProduct> ActiveFlashSales { get; set; } = new();
        public List<NearGo.Models.Supermarket> Supermarkets { get; set; } = new();
        public List<SurpriseBox> SurpriseBoxes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;

            Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Take(16)
                .ToListAsync();

            Banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            FeaturedProducts = await _context.Products
                .Include(p => p.Supermarket)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity > 0 && p.ExpiryDate > now)
                .OrderByDescending(p => p.IsBoosted)
                .ThenByDescending(p => p.ViewCount)
                .Take(15)
                .ToListAsync();

            var activeFlashSale = await _context.FlashSales
                .Where(f => f.IsActive && f.StartTime <= now && f.EndTime > now)
                .FirstOrDefaultAsync();

            if (activeFlashSale != null)
            {
                ActiveFlashSales = await _context.FlashSaleProducts
                    .Include(fp => fp.Product).ThenInclude(p => p.Supermarket)
                    .Where(fp => fp.FlashSaleId == activeFlashSale.Id && fp.Product.IsActive && fp.Product.StockQuantity > 0)
                    .Take(10)
                    .ToListAsync();
            }

            Supermarkets = await _context.Supermarkets
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.Rating)
                .Take(12)
                .ToListAsync();

            SurpriseBoxes = await _context.SurpriseBoxes
                .Where(sb => sb.IsActive && sb.StockQuantity > 0)
                .Take(3)
                .ToListAsync();
        }
    }
}
