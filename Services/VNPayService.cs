using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using NearGo.Configurations;

namespace NearGo.Services
{
    public class VNPayService
    {
        private readonly VNPaySettings _settings;

        public VNPayService(IOptions<VNPaySettings> settings)
        {
            _settings = settings.Value;
        }

        public string CreatePaymentUrl(decimal amount, string orderCode, string ipAddress)
        {
            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _settings.TmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", orderCode },
                { "vnp_OrderInfo", $"Thanh toan don hang {orderCode}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _settings.ReturnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") }
            };

            var signData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            var signature = HmacSha512(_settings.HashSecret, signData);
            vnpParams.Add("vnp_SecureHash", signature);

            var queryString = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            return $"{_settings.BaseUrl}?{queryString}";
        }

        public bool VerifyReturnUrl(IQueryCollection query)
        {
            var vnpParams = new SortedDictionary<string, string>();
            foreach (var kvp in query)
            {
                if (kvp.Key.StartsWith("vnp_") && kvp.Key != "vnp_SecureHash")
                {
                    vnpParams.Add(kvp.Key, kvp.Value.ToString());
                }
            }

            var receivedHash = query["vnp_SecureHash"].ToString();
            var signData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            var computedHash = HmacSha512(_settings.HashSecret, signData);

            return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }
    }
}
