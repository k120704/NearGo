using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Services;

namespace NearGo.Pages.Payment
{
    public class VNPayReturnModel : PageModel
    {
        private readonly VNPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly OrderService _orderService;

        public VNPayReturnModel(VNPayService vnPayService, ApplicationDbContext context, OrderService orderService)
        {
            _vnPayService = vnPayService;
            _context = context;
            _orderService = orderService;
        }

        public bool IsSuccess { get; set; }
        public bool IsBannerPayment { get; set; }
        public bool IsSubscriptionPayment { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var isValid = _vnPayService.VerifyReturnUrl(Request.Query);
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnp_TransactionNo = Request.Query["vnp_TransactionNo"].ToString();
            var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();
            var vnp_BankCode = Request.Query["vnp_BankCode"].ToString();
            var vnp_Amount = Request.Query["vnp_Amount"].ToString();

            if (isValid && vnp_ResponseCode == "00")
            {
                IsSuccess = true;
                TransactionId = vnp_TransactionNo;

                if (vnp_TxnRef.StartsWith("BNR-"))
                {
                    IsBannerPayment = true;
                    var parts = vnp_TxnRef.Split('-');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var bannerId))
                    {
                        var banner = await _context.Banners.FindAsync(bannerId);
                        if (banner != null)
                        {
                            banner.PaymentStatus = "Paid";
                            banner.Status = "Pending";
                            banner.TransactionId = vnp_TransactionNo;
                            await _context.SaveChangesAsync();
                        }
                    }
                    TempData["Success"] = "Thanh toán banner thành công! Đã gửi yêu cầu chờ admin duyệt.";
                    return RedirectToPage("/Supermarket/Banners");
                }

                if (vnp_TxnRef.StartsWith("BATCH-"))
                {
                    var sessionId = vnp_TxnRef["BATCH-".Length..];
                    var orders = await _orderService.GetOrdersBySessionId(sessionId);

                    foreach (var order in orders)
                    {
                        order.PaymentStatus = "Paid";
                        order.Status = "Pending";
                        order.PaymentDate = DateTime.UtcNow;
                        order.TransactionId = vnp_TransactionNo;
                        order.PaymentMethod = "VNPay";

                        await _orderService.CreatePaymentTransaction(
                            order.Id, "VNPay", vnp_TransactionNo, vnp_BankCode,
                            order.TotalAmount, "Success", vnp_ResponseCode, "Giao dịch thành công");
                    }
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thanh toán thành công! Đơn hàng đang được xử lý.";
                    return RedirectToPage("/Customer/Orders");
                }

                if (vnp_TxnRef.StartsWith("SUB-"))
                {
                    IsSubscriptionPayment = true;
                    var parts = vnp_TxnRef.Split('-');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var supermarketId))
                    {
                        var supermarket = await _context.Supermarkets.FindAsync(supermarketId);
                        if (supermarket != null && supermarket.SubscriptionTier != "Premium")
                        {
                            var now = DateTime.UtcNow;

                            supermarket.SubscriptionTier = "Premium";
                            supermarket.SubscriptionExpiry = now.AddMonths(1);

                            _context.Subscriptions.Add(new Models.Subscription
                            {
                                SupermarketId = supermarket.Id,
                                Tier = "Premium",
                                Amount = 199000,
                                StartDate = now,
                                EndDate = now.AddMonths(1),
                                Status = "Active",
                                CreatedAt = now
                            });

                            _context.PlatformFees.Add(new Models.PlatformFee
                            {
                                SupermarketId = supermarket.Id,
                                FeeType = "Subscription",
                                Amount = 199000,
                                Description = $"Đăng ký gói Premium 1 tháng - GD: {vnp_TransactionNo}",
                                Status = "Paid",
                                CreatedAt = now,
                                PaidAt = now
                            });

                            await _context.SaveChangesAsync();

                            TempData["Success"] = "Nâng cấp lên gói Premium thành công! Giờ bạn có thể đăng sản phẩm không giới hạn.";
                            return RedirectToPage("/Supermarket/Dashboard");
                        }
                    }

                    TempData["Info"] = "Không tìm thấy siêu thị hoặc đã ở gói Premium.";
                    return RedirectToPage("/Supermarket/Upgrade");
                }

                var orderByCode = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == vnp_TxnRef);
                if (orderByCode != null)
                {
                    orderByCode.PaymentStatus = "Paid";
                    orderByCode.Status = "Pending";
                    orderByCode.PaymentDate = DateTime.UtcNow;
                    orderByCode.TransactionId = vnp_TransactionNo;
                    orderByCode.PaymentMethod = "VNPay";

                    await _orderService.CreatePaymentTransaction(
                        orderByCode.Id, "VNPay", vnp_TransactionNo, vnp_BankCode,
                        orderByCode.TotalAmount, "Success", vnp_ResponseCode, "Giao dịch thành công");

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thanh toán thành công!";
                    return RedirectToPage("/Customer/Orders");
                }
            }
            else
            {
                IsSuccess = false;
                ErrorMessage = vnp_ResponseCode switch
                {
                    "01" => "Giao dịch chưa hoàn tất",
                    "02" => "Lỗi hệ thống",
                    "04" => "Giao dịch đảo ngược",
                    "24" => "Khách hàng hủy giao dịch",
                    _ => "Thanh toán không thành công"
                };
            }

            return Page();
        }
    }
}
