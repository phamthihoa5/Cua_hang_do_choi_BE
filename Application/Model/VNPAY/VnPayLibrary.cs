using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Application.Common.VNPAY
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new();
        private readonly SortedList<string, string> _responseData = new();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData.Add(key, value);
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                    data.AppendFormat("{0}={1}&", kv.Key, HttpUtility.UrlEncode(kv.Value));
            }

            var rawData = data.ToString().TrimEnd('&');
            var secureHash = HmacSHA512(hashSecret, rawData);
            return $"{baseUrl}?{rawData}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rawData = string.Join("&", _responseData
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .Select(kv => kv.Key + "=" + kv.Value));

            var computedHash = HmacSHA512(secretKey, rawData);
            return computedHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSHA512(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
