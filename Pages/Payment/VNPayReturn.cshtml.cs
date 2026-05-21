using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Services;

namespace NearGo.Pages.Payment
{
    public class VNPayReturnModel : PageModel
    {
        private readonly VNPayService _vnPayService;

        public VNPayReturnModel(VNPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var isValid = _vnPayService.VerifyReturnUrl(Request.Query);
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnp_TransactionNo = Request.Query["vnp_TransactionNo"].ToString();
            var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();

            if (isValid && vnp_ResponseCode == "00")
            {
                IsSuccess = true;
                TransactionId = vnp_TransactionNo;
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
