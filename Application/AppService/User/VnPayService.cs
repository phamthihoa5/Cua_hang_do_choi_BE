using Application.Common.VNPAY;
using Application.IService.User;
using Microsoft.Extensions.Configuration;
using VNPAY.NET;

namespace Application.AppService.User
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _cfg;
        private readonly string _tmn;
        private readonly string _hashSecret;
        private readonly string _baseUrl;
        private readonly string _returnUrl;

        public VnPayService(IConfiguration cfg)
        {
            _cfg = cfg;
            _tmn = cfg["VnPay:TmnCode"];
            _hashSecret = cfg["VnPay:HashSecret"];
            _baseUrl = cfg["VnPay:BaseUrl"];
            _returnUrl = cfg["VnPay:ReturnUrl"];
        }

        public Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string orderInfo, string bankCode = "")
        {
            var vnp = new VnPayLibrary();

            vnp.AddRequestData("vnp_Version", "2.1.0");
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", _tmn);
            vnp.AddRequestData("vnp_Amount", ((long)amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", "VND");
            vnp.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnp.AddRequestData("vnp_Locale", "vn");
            vnp.AddRequestData("vnp_OrderInfo", orderInfo);
            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", _returnUrl);
            vnp.AddRequestData("vnp_TxnRef", orderId);

            if (!string.IsNullOrEmpty(bankCode))
                vnp.AddRequestData("vnp_BankCode", bankCode);

            string paymentUrl = vnp.CreateRequestUrl(_baseUrl, _hashSecret);
            return Task.FromResult(paymentUrl);
        }

        public Task<bool> ValidateCallbackAsync(IDictionary<string, string> queryParams)
        {
            var vnp = new VnPayLibrary();
            foreach (var kv in queryParams)
                vnp.AddResponseData(kv.Key, kv.Value);

            string secureHash = vnp.GetResponseData("vnp_SecureHash");
            bool isValid = vnp.ValidateSignature(secureHash, _hashSecret);
            return Task.FromResult(isValid);
        }
    }
}
