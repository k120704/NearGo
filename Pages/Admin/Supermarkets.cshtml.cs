using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SupermarketsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SupermarketsModel(ApplicationDbContext context) => _context = context;

        public List<NearGo.Models.Supermarket> Supermarkets { get; set; } = new();

        public async Task OnGetAsync()
        {
            Supermarkets = await _context.Supermarkets
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
