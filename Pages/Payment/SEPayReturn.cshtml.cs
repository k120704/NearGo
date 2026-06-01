using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Services;

namespace NearGo.Pages.Payment
{
    public class SEPayReturnModel : PageModel
    {
        private readonly SEPayService _sePayService;
        private readonly ApplicationDbContext _context;

        public SEPayReturnModel(SEPayService sePayService, ApplicationDbContext context)
        {
            _sePayService = sePayService;
            _context = context;
        }

        public string OrderCode { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public string TransferContent { get; set; } = string.Empty;
        public string QRUrl { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public async Task<IActionResult> OnGet(string orderCode, decimal? amount = null)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return RedirectToPage("/NotFound");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order != null)
            {
                Amount = order.TotalAmount;
            }
            else if (amount.HasValue)
            {
                Amount = amount.Value;
            }
            else
            {
                return RedirectToPage("/NotFound");
            }

            OrderCode = orderCode;
            BankAccount = _sePayService.GetBankAccount();
            BankName = _sePayService.GetBankName();
            AccountHolder = _sePayService.GetAccountHolder();
            TransferContent = _sePayService.GenerateTransferContent(orderCode);
            QRUrl = _sePayService.GenerateQRUrl(orderCode);

            return Page();
        }
    }
}
