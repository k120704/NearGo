using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CategoriesModel(ApplicationDbContext context) => _context = context;

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(string name, string? slug, string? description, string? iconClass, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Vui lòng nhập tên danh mục.";
                return RedirectToPage();
            }

            _context.Categories.Add(new Category
            {
                Name = name,
                Slug = string.IsNullOrWhiteSpace(slug) ? name.ToLower().Replace(" ", "-") : slug,
                Description = description,
                IconClass = iconClass,
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm danh mục \"{name}\"";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, string name, string? slug, string? description, string? iconClass, int sortOrder, bool isActive)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Vui lòng nhập tên danh mục.";
                return RedirectToPage();
            }

            cat.Name = name;
            cat.Slug = string.IsNullOrWhiteSpace(slug) ? name.ToLower().Replace(" ", "-") : slug;
            cat.Description = description;
            cat.IconClass = iconClass;
            cat.SortOrder = sortOrder;
            cat.IsActive = isActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật danh mục \"{name}\"";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var cat = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();

            if (cat.Products.Count > 0)
            {
                TempData["Error"] = $"Không thể xóa danh mục \"{cat.Name}\" vì còn {cat.Products.Count} sản phẩm.";
                return RedirectToPage();
            }

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa danh mục \"{cat.Name}\"";
            return RedirectToPage();
        }
    }
}
