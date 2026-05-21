using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Cart
{
    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(CartService cartService, UserManager<AppUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public List<NearGo.Models.CartItem> CartItems { get; set; } = new();
        public decimal Total { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            CartItems = await _cartService.GetCartItems(userId);
            Total = _cartService.CalculateCartTotal(CartItems);
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _cartService.GetCartItems(userId);
            if (!items.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToPage();
            }
            return RedirectToPage("/Checkout/Index");
        }
    }
}
