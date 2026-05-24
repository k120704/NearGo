using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Supermarket
{
    [Authorize(Roles = "Supermarket")]
    public class UpgradeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly VNPayService _vnPayService;

        public UpgradeModel(ApplicationDbContext context, UserManager<AppUser> userManager, VNPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
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

            var txnRef = $"SUB-{supermarket.Id}";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(199000, txnRef, ipAddress);

            return Redirect(paymentUrl);
        }
    }
}
