using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SupermarketsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SupermarketsModel(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Models.Supermarket> Supermarkets { get; set; } = new();
        public string? Filter { get; set; }
        public string? Search { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public const int PageSize = 10;

        public async Task OnGetAsync(string? filter, string? search, int? p)
        {
            Filter = filter;
            Search = search;
            CurrentPage = p ?? 1;

            var query = _context.Supermarkets.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s)
                    || x.Email.ToLower().Contains(s)
                    || (x.Phone ?? "").Contains(s));
            }

            query = filter switch
            {
                "active" => query.Where(x => x.IsActive),
                "inactive" => query.Where(x => !x.IsActive),
                _ => query
            };

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (TotalPages < 1) TotalPages = 1;
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            Supermarkets = await query
                .OrderByDescending(s => s.CreatedAt)
                .ThenBy(s => s.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id, string? filter, string? search, int? p)
        {
            var sm = await _context.Supermarkets.FindAsync(id);
            if (sm == null) return NotFound();

            sm.IsActive = !sm.IsActive;

            var user = await _userManager.FindByEmailAsync(sm.Email);
            if (user != null)
            {
                if (sm.IsActive)
                {
                    user.LockoutEnabled = false;
                    user.LockoutEnd = null;
                }
                else
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = sm.IsActive
                ? $"Đã mở khóa siêu thị \"{sm.Name}\""
                : $"Đã khóa siêu thị \"{sm.Name}\"";
            return RedirectToPage("Supermarkets", new { filter, search, p });
        }

    }
}
