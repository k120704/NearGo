using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class FeesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public FeesModel(ApplicationDbContext context) => _context = context;
        public List<NearGo.Models.PlatformFee> Fees { get; set; } = new();
        public async Task OnGetAsync()
        {
            Fees = await _context.PlatformFees
                .Include(f => f.Supermarket)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
