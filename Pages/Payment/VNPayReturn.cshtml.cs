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

        public VNPayReturnModel(VNPayService vnPayService, ApplicationDbContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        public bool IsSuccess { get; set; }
        public bool IsBannerPayment { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var isValid = _vnPayService.VerifyReturnUrl(Request.Query);
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnp_TransactionNo = Request.Query["vnp_TransactionNo"].ToString();
            var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();

            if (isValid && vnp_ResponseCode == "00")
            {
                IsSuccess = true;
                TransactionId = vnp_TransactionNo;

                if (vnp_TxnRef.StartsWith("BNR-"))
                {
                    var parts = vnp_TxnRef.Split('-');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var bannerId))
                    {
                        var banner = await _context.Banners.FindAsync(bannerId);
                        if (banner != null)
                        {
                            banner.PaymentStatus = "Paid";
                            banner.Status = "Pending";
                            banner.TransactionId = vnp_TxnRef;
                            await _context.SaveChangesAsync();
                        }
                    }
                    TempData["Success"] = "Thanh toán banner thành công! Đã gửi yêu cầu chờ admin duyệt.";
                    return RedirectToPage("/Supermarket/Banners");
                }
                else
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == vnp_TxnRef);
                    if (order != null)
                    {
                        order.PaymentStatus = "Paid";
                        order.Status = "Pending";
                        await _context.SaveChangesAsync();
                    }
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
