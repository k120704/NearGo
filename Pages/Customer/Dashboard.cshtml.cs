using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public AppUser? UserProfile { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int WishlistCount { get; set; }
        public int LoyaltyPoints { get; set; }
        public List<Order> RecentOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            UserProfile = await _userManager.GetUserAsync(User);

            TotalOrders = await _context.Orders.CountAsync(o => o.CustomerId == userId);
            CompletedOrders = await _context.Orders.CountAsync(o => o.CustomerId == userId && o.Status == "Delivered");
            WishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId);
            LoyaltyPoints = (await _context.LoyaltyPoints
                .Where(lp => lp.UserId == userId && lp.ExpiryDate > DateTime.UtcNow)
                .SumAsync(lp => (int?)lp.Points)) ?? 0;

            RecentOrders = await _context.Orders
                .Include(o => o.Supermarket)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
        }
    }
}
