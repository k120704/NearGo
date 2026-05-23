using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Customer.Orders
{
    [Authorize(Roles = "Customer")]
    public class DetailModel : PageModel
    {
        private readonly OrderService _orderService;
        private readonly UserManager<AppUser> _userManager;

        public DetailModel(OrderService orderService, UserManager<AppUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderById(id);

            if (order == null || order.CustomerId != userId)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }
    }
}
