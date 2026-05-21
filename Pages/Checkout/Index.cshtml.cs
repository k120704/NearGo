using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Models;
using NearGo.Services;
using System.ComponentModel.DataAnnotations;

namespace NearGo.Pages.Checkout
{
    [Authorize(Roles = "Customer")]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(CartService cartService, OrderService orderService, UserManager<AppUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<NearGo.Models.CartItem> CartItems { get; set; } = new();
        public decimal SubTotal { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
            public string CustomerName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string CustomerPhone { get; set; } = string.Empty;

            [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
            public string ShippingAddress { get; set; } = string.Empty;

            public string? Note { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
            public string PaymentMethod { get; set; } = "COD";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            CartItems = await _cartService.GetCartItems(userId);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }
            SubTotal = _cartService.CalculateCartTotal(CartItems);

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Input.CustomerName = user.FullName;
                Input.CustomerPhone = user.PhoneNumber ?? "";
                Input.ShippingAddress = user.Address ?? "";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return await OnGetAsync();

            var userId = _userManager.GetUserId(User)!;
            CartItems = await _cartService.GetCartItems(userId);
            if (!CartItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }
            SubTotal = _cartService.CalculateCartTotal(CartItems);

            var supermarketIds = CartItems.Select(c => c.Product.SupermarketId).Distinct();
            foreach (var smId in supermarketIds)
            {
                var order = await _orderService.CreateOrder(
                    userId, smId, Input.ShippingAddress,
                    Input.CustomerName, Input.CustomerPhone, Input.Note);

                if (Input.PaymentMethod == "VNPay")
                {
                    var vnpay = HttpContext.RequestServices.GetRequiredService<VNPayService>();
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    var paymentUrl = vnpay.CreatePaymentUrl(order.TotalAmount, order.OrderCode, ipAddress);
                    return Redirect(paymentUrl);
                }
                else if (Input.PaymentMethod == "Momo")
                {
                    var momo = HttpContext.RequestServices.GetRequiredService<MomoService>();
                    var paymentUrl = await momo.CreatePaymentUrl(order.TotalAmount, order.OrderCode, Input.CustomerName, Input.CustomerPhone);
                    return Redirect(paymentUrl);
                }
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToPage("/Customer/Orders");
        }
    }
}
