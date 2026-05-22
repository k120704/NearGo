using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Models;
using NearGo.Services;

namespace NearGo.Pages.Cart
{
    [IgnoreAntiforgeryToken]
    public class CartApiModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly UserManager<AppUser> _userManager;

        public CartApiModel(CartService cartService, UserManager<AppUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetCountAsync()
        {
            if (!User.Identity?.IsAuthenticated == true) return Content("0");
            var userId = _userManager.GetUserId(User)!;
            var count = await _cartService.GetCartCount(userId);
            return Content(count.ToString());
        }

        public async Task<IActionResult> OnPostAddAsync(int productId, int quantity = 1)
        {
            if (!User.Identity?.IsAuthenticated == true)
                return new JsonResult(new { success = false, message = "Vui lòng đăng nhập" });

            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.AddToCart(userId, productId, quantity);
            if (result == null)
                return new JsonResult(new { success = false, message = "Sản phẩm không tồn tại hoặc đã hết hàng" });

            var count = await _cartService.GetCartCount(userId);
            return new JsonResult(new { success = true, count });
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, int quantity)
        {
            if (!User.Identity?.IsAuthenticated == true) return new JsonResult(new { success = false });
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.UpdateQuantity(userId, id, quantity);
            return new JsonResult(new { success = result });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            if (!User.Identity?.IsAuthenticated == true) return new JsonResult(new { success = false });
            var userId = _userManager.GetUserId(User)!;
            var result = await _cartService.RemoveFromCart(userId, id);
            return new JsonResult(new { success = result });
        }
    }
}
