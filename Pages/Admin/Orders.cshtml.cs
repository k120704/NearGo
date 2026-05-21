using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;

namespace NearGo.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public OrdersModel(ApplicationDbContext context) => _context = context;
        public List<NearGo.Models.Order> Orders { get; set; } = new();
        public async Task OnGetAsync()
        {
            Orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Supermarket)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}
