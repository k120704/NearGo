using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NearGo.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NearGo.Services
{
    public class MomoService
    {
        private readonly MomoSettings _settings;
        private readonly HttpClient _httpClient;

        public MomoService(IOptions<MomoSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
        }

        public async Task<string> CreatePaymentUrl(decimal amount, string orderCode, string customerName, string customerPhone)
        {
            var requestId = Guid.NewGuid().ToString();
            var rawSignature = $"accessKey={_settings.AccessKey}&amount={(long)amount}&extraData=&ipnUrl={_settings.NotifyUrl}&orderId={requestId}&orderInfo=Thanh toán đơn hàng {orderCode}&partnerCode={_settings.PartnerCode}&redirectUrl={_settings.ReturnUrl}&requestId={requestId}&requestType=captureWallet";

            var signature = HmacSha256(_settings.SecretKey, rawSignature);

            var payload = new
            {
                partnerCode = _settings.PartnerCode,
                partnerName = "NearGo",
                storeId = "NearGo",
                requestId,
                amount = (long)amount,
                orderId = requestId,
                orderInfo = $"Thanh toán đơn hàng {orderCode}",
                redirectUrl = _settings.ReturnUrl,
                ipnUrl = _settings.NotifyUrl,
                lang = "vi",
                extraData = "",
                requestType = "captureWallet",
                signature,
                orderGroupId = ""
            };

            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_settings.ApiEndpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseJson);

            if (result != null && result.payUrl != null)
            {
                return result.payUrl.ToString();
            }

            throw new Exception($"Momo payment creation failed: {responseJson}");
        }

        public bool VerifyReturnUrl(IQueryCollection query)
        {
            var receivedSignature = query["signature"].ToString();
            var rawSignature = $"accessKey={_settings.AccessKey}&amount={query["amount"]}&extraData={query["extraData"]}&message={query["message"]}&orderId={query["orderId"]}&orderInfo={query["orderInfo"]}&orderType={query["orderType"]}&partnerCode={query["partnerCode"]}&payType={query["payType"]}&requestId={query["requestId"]}&responseTime={query["responseTime"]}&resultCode={query["resultCode"]}&transId={query["transId"]}";

            var computedSignature = HmacSha256(_settings.SecretKey, rawSignature);
            return computedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }
    }
}
