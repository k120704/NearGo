using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;
using NearGo.Services;
using System.ComponentModel.DataAnnotations;

namespace NearGo.Pages.Supermarket.Banners
{
    [Authorize(Roles = "Supermarket")]
    public class PaymentModel : PageModel
    {
        private static readonly Dictionary<int, decimal> PackagePrices = new()
        {
            { 7, 50000 },
            { 15, 100000 },
            { 30, 200000 }
        };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PaymentModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;
            [StringLength(500)]
            public string? Subtitle { get; set; }
            [Required(ErrorMessage = "Hình ảnh là bắt buộc")]
            [Url]
            public string ImageUrl { get; set; } = string.Empty;
            public int PackageDays { get; set; } = 30;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return Forbid();

            var banner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id && b.SupermarketId == user.SupermarketId.Value
                    && b.Status == "Draft" && b.PaymentStatus == "Unpaid");
            if (banner == null)
            {
                TempData["Error"] = "Banner không hợp lệ hoặc đã thanh toán";
                return RedirectToPage("/Supermarket/Banners");
            }

            Input.Title = banner.Title;
            Input.Subtitle = banner.Subtitle;
            Input.ImageUrl = banner.ImageUrl;
            Input.PackageDays = banner.PackageDays;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return Forbid();

            var banner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id && b.SupermarketId == user.SupermarketId.Value
                    && b.Status == "Draft" && b.PaymentStatus == "Unpaid");
            if (banner == null) return NotFound();

            if (!PackagePrices.TryGetValue(Input.PackageDays, out var price))
            {
                ModelState.AddModelError("Input.PackageDays", "Gói không hợp lệ");
                return Page();
            }

            banner.Title = Input.Title;
            banner.Subtitle = Input.Subtitle;
            banner.ImageUrl = Input.ImageUrl;
            banner.PackageDays = Input.PackageDays;
            banner.PackagePrice = price;

            var vnpay = HttpContext.RequestServices.GetRequiredService<VNPayService>();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var txnRef = $"BNR-{banner.Id}-{Input.PackageDays}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            banner.TransactionId = txnRef;
            await _context.SaveChangesAsync();

            var paymentUrl = vnpay.CreatePaymentUrl(price, txnRef, ipAddress);
            return Redirect(paymentUrl);
        }
    }
}
