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
    public class BannersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BannersModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<NearGo.Models.Banner> Banners { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return;
            Banners = await _context.Banners
                .Where(b => b.SupermarketId == user.SupermarketId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return Forbid();

            var banner = new NearGo.Models.Banner
            {
                Title = "Banner mới",
                ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=1920",
                SupermarketId = user.SupermarketId.Value,
                SortOrder = 0,
                IsActive = false,
                Status = "Draft",
                PaymentStatus = "Unpaid",
                PackageDays = 30,
                PackagePrice = 200000
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Supermarket/Banners/Payment", new { id = banner.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SupermarketId == null) return Forbid();

            var banner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Id == id && b.SupermarketId == user.SupermarketId.Value);
            if (banner == null) return NotFound();

            if (banner.Status == "Approved" || banner.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Không thể xóa banner đã thanh toán hoặc đang hiển thị";
                return RedirectToPage("Banners");
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa banner";
            return RedirectToPage("Banners");
        }
    }
}
