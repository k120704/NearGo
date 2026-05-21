using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CategoriesModel(ApplicationDbContext context) => _context = context;
        public List<NearGo.Models.Category> Categories { get; set; } = new();
        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }
    }
}
