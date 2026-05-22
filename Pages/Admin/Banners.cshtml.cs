using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BannersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BannersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<NearGo.Models.Banner> Banners { get; set; } = new();
        public int TotalCount { get; set; }
        public string? Filter { get; set; }

        public async Task OnGetAsync(string? filter)
        {
            Filter = filter;

            var query = _context.Banners.Include(b => b.Supermarket)
                .Where(b => b.PaymentStatus == "Paid")
                .AsQueryable();

            if (filter == "Active")
                query = query.Where(b => b.IsActive && b.Status == "Approved");
            else if (filter == "Approved")
                query = query.Where(b => b.Status == "Approved");
            else if (filter == "Rejected")
                query = query.Where(b => b.Status == "Rejected");
            else if (filter == "Pending")
                query = query.Where(b => b.Status == "Pending");

            TotalCount = await query.CountAsync();
            Banners = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            if (banner.Status != "Pending" || banner.PaymentStatus != "Paid")
            {
                TempData["Error"] = "Chỉ có thể duyệt banner đã thanh toán";
                return RedirectToPage("Banners");
            }

            banner.Status = "Approved";

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã duyệt banner: {banner.Title}";
            return RedirectToPage("Banners");
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? note)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            if (banner.Status != "Pending" || banner.PaymentStatus != "Paid")
            {
                TempData["Error"] = "Chỉ có thể từ chối banner đã thanh toán";
                return RedirectToPage("Banners");
            }

            banner.Status = "Rejected";
            banner.AdminNote = note;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã từ chối banner: {banner.Title}";
            return RedirectToPage("Banners");
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            if (banner.Status != "Approved")
            {
                TempData["Error"] = "Chỉ có thể ẩn/hiện banner đã duyệt";
                return RedirectToPage("Banners");
            }

            banner.IsActive = !banner.IsActive;

            if (banner.IsActive)
            {
                var maxSort = await _context.Banners
                    .Where(b => b.IsActive && b.Status == "Approved")
                    .MaxAsync(b => (int?)b.SortOrder) ?? 0;
                banner.SortOrder = maxSort + 1;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = banner.IsActive
                ? $"Banner \"{banner.Title}\" đã được đưa lên website"
                : $"Đã ẩn banner \"{banner.Title}\" khỏi website";
            return RedirectToPage("Banners");
        }

        public async Task<IActionResult> OnPostMoveUpAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null || !banner.IsActive) return NotFound();

            var above = await _context.Banners
                .Where(b => b.IsActive && b.Status == "Approved" && b.SortOrder < banner.SortOrder)
                .OrderByDescending(b => b.SortOrder)
                .FirstOrDefaultAsync();

            if (above != null)
            {
                (banner.SortOrder, above.SortOrder) = (above.SortOrder, banner.SortOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Banners");
        }

        public async Task<IActionResult> OnPostMoveDownAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null || !banner.IsActive) return NotFound();

            var below = await _context.Banners
                .Where(b => b.IsActive && b.Status == "Approved" && b.SortOrder > banner.SortOrder)
                .OrderBy(b => b.SortOrder)
                .FirstOrDefaultAsync();

            if (below != null)
            {
                (banner.SortOrder, below.SortOrder) = (below.SortOrder, banner.SortOrder);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Banners");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa banner";
            return RedirectToPage("Banners");
        }
    }
}
