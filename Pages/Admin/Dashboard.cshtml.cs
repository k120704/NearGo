using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalSupermarkets { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<NearGo.Models.Supermarket> TopSupermarkets { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            TotalSupermarkets = await _context.Supermarkets.CountAsync();
            TotalOrders = await _context.Orders.CountAsync();
            TotalRevenue = await _context.PlatformFees
                .Where(f => f.Status == "Paid")
                .SumAsync(f => (decimal?)f.Amount) ?? 0;

            TopSupermarkets = await _context.Supermarkets
                .OrderByDescending(s => s.TotalOrders)
                .Take(5)
                .ToListAsync();
        }
    }
}
