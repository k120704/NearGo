using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.FlashSales
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<FlashSaleProduct> FlashSaleProducts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;
            var activeSales = await _context.FlashSales
                .Where(f => f.IsActive && f.StartTime <= now && f.EndTime > now)
                .ToListAsync();

            foreach (var sale in activeSales)
            {
                var products = await _context.FlashSaleProducts
                    .Include(fp => fp.Product).ThenInclude(p => p.Supermarket)
                    .Where(fp => fp.FlashSaleId == sale.Id && fp.Product.IsActive && fp.Product.StockQuantity > 0 && fp.MaxQuantity > fp.SoldQuantity)
                    .ToListAsync();
                FlashSaleProducts.AddRange(products);
            }
        }
    }
}
