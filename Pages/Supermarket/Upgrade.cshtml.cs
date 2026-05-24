using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.Supermarket
{
    [Authorize(Roles = "Supermarket")]
    public class UpgradeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UpgradeModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public NearGo.Models.Supermarket? Supermarket { get; set; }
        public int ProductsThisMonth { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return;

            Supermarket = await _context.Supermarkets.FindAsync(user.SupermarketId.Value);
            if (Supermarket == null) return;

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            ProductsThisMonth = await _context.Products
                .CountAsync(p => p.SupermarketId == Supermarket.Id && p.CreatedAt >= startOfMonth);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return Forbid();

            var supermarket = await _context.Supermarkets.FindAsync(user.SupermarketId.Value);
            if (supermarket == null) return Forbid();

            if (supermarket.SubscriptionTier == "Premium")
            {
                TempData["Info"] = "Siêu thị của bạn đã ở gói Premium.";
                return RedirectToPage("/Supermarket/Upgrade");
            }

            var now = DateTime.UtcNow;

            supermarket.SubscriptionTier = "Premium";
            supermarket.SubscriptionExpiry = now.AddMonths(1);

            _context.Subscriptions.Add(new Subscription
            {
                SupermarketId = supermarket.Id,
                Tier = "Premium",
                Amount = 199000,
                StartDate = now,
                EndDate = now.AddMonths(1),
                Status = "Active",
                CreatedAt = now
            });

            _context.PlatformFees.Add(new PlatformFee
            {
                SupermarketId = supermarket.Id,
                FeeType = "Subscription",
                Amount = 199000,
                Description = "Đăng ký gói Premium 1 tháng",
                Status = "Paid",
                CreatedAt = now,
                PaidAt = now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Nâng cấp lên gói Premium thành công! Giờ bạn có thể đăng sản phẩm không giới hạn.";
            return RedirectToPage("/Supermarket/Dashboard");
        }
    }
}
