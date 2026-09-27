using Application.Interface;
using Application.IService;
using Application.IService.User;
using Application.Model.Ghn;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class GhnService : IGhnService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderNotifier? _orderNotifier;
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly string _shopId;
        private readonly int _fromDistrictId;
        private readonly string _fromWardCode;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public GhnService(IConfiguration config, IUnitOfWork unitOfWork, IOrderNotifier? orderNotifier = null, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _config = config;
            _unitOfWork = unitOfWork;
            _orderNotifier = orderNotifier;

            _baseUrl = _config["GHN:BaseUrl"] ?? "https://online-gateway.ghn.vn/shiip/public-api/";
            _token = (_config["GHN:Token"] ?? "820e0527-b332-11f1-9109-4605045490dc").Trim();
            _shopId = (_config["GHN:ShopId"] ?? "6675009").Trim();
            _fromDistrictId = int.TryParse(_config["GHN:FromDistrictId"], out var dist) ? dist : 1482;
            _fromWardCode = _config["GHN:FromWardCode"] ?? "11013";
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, object body = null, bool includeShopId = false)
        {
            var url = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
            var request = new HttpRequestMessage(method, url);
            request.Headers.TryAddWithoutValidation("Token", _token);

            if (includeShopId && !string.IsNullOrEmpty(_shopId))
            {
                request.Headers.TryAddWithoutValidation("ShopId", _shopId);
            }

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string endpoint, object body = null, bool includeShopId = false)
        {
            var request = CreateRequest(method, endpoint, body, includeShopId);
            var response = await _httpClient.SendAsync(request);

            // Tự động Fallback sang Production API cho Master Data / Fee nếu dùng Dev URL bị lỗi 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && _baseUrl.Contains("dev-online-gateway"))
            {
                var fallbackUrl = $"https://online-gateway.ghn.vn/shiip/public-api/{endpoint.TrimStart('/')}";
                var fallbackReq = new HttpRequestMessage(method, fallbackUrl);
                fallbackReq.Headers.TryAddWithoutValidation("Token", "820e0527-b332-11f1-9109-4605045490dc");
                if (includeShopId) fallbackReq.Headers.TryAddWithoutValidation("ShopId", "6675009");
                if (body != null)
                {
                    fallbackReq.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                }
                return await _httpClient.SendAsync(fallbackReq);
            }

            return response;
        }

        public async Task<List<GhnProvinceDto>> GetProvincesAsync()
        {
            try
            {
                var response = await SendRequestAsync(HttpMethod.Get, "master-data/province", includeShopId: false);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GhnResponse<List<GhnProvinceDto>>>(json, _jsonOptions);
                if (result?.Code == 200 && result?.Data != null)
                {
                    return result.Data;
                }

                Console.WriteLine($"GHN GetProvinces error response: {json}");
                return new List<GhnProvinceDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching provinces from GHN: {ex.Message}");
                return new List<GhnProvinceDto>();
            }
        }

        public async Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId)
        {
            try
            {
                var response = await SendRequestAsync(HttpMethod.Post, "master-data/district", new { province_id = provinceId }, includeShopId: false);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GhnResponse<List<GhnDistrictDto>>>(json, _jsonOptions);
                if (result?.Code == 200 && result?.Data != null)
                {
                    return result.Data;
                }

                Console.WriteLine($"GHN GetDistricts error response: {json}");
                return new List<GhnDistrictDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching districts from GHN: {ex.Message}");
                return new List<GhnDistrictDto>();
            }
        }

        public async Task<List<GhnWardDto>> GetWardsAsync(int districtId)
        {
            try
            {
                var response = await SendRequestAsync(HttpMethod.Post, "master-data/ward", new { district_id = districtId }, includeShopId: false);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GhnResponse<List<GhnWardDto>>>(json, _jsonOptions);
                if (result?.Code == 200 && result?.Data != null)
                {
                    return result.Data;
                }

                Console.WriteLine($"GHN GetWards error response: {json}");
                return new List<GhnWardDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching wards from GHN: {ex.Message}");
                return new List<GhnWardDto>();
            }
        }

        public async Task<decimal> CalculateShippingFeeAsync(int toDistrictId, string toWardCode, int weight = 500)
        {
            try
            {
                var body = new
                {
                    from_district_id = _fromDistrictId,
                    from_ward_code = _fromWardCode,
                    to_district_id = toDistrictId,
                    to_ward_code = toWardCode,
                    weight = weight > 0 ? weight : 500,
                    length = 20,
                    width = 15,
                    height = 10,
                    service_type_id = 2
                };

                var response = await SendRequestAsync(HttpMethod.Post, "v2/shipping-order/fee", body, includeShopId: false);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GhnResponse<GhnFeeDataDto>>(json, _jsonOptions);
                if (result != null && result.Code == 200 && result.Data != null)
                {
                    var calculatedFee = result.Data.Total > 0 ? result.Data.Total : result.Data.ServiceFee;
                    Console.WriteLine($"✅ GHN Fee calculated successfully: {calculatedFee} VND for District {toDistrictId}");
                    return calculatedFee;
                }

                Console.WriteLine($"GHN Fee calculation response error: {json}");
                return 30000; // Fallback fee if error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating GHN fee: {ex.Message}");
                return 30000;
            }
        }

        public async Task<GhnCreateOrderData> CreateShippingOrderAsync(
            Order order,
            int toDistrictId,
            string toWardCode,
            string toAddress,
            string recipientName,
            string recipientPhone,
            decimal codAmount = 0)
        {
            try
            {
                var items = order.OrderDetail?.Select(d => new GhnOrderItem
                {
                    Name = d.Product?.ProductName ?? "Do choi",
                    Quantity = d.Quantity,
                    Price = (int)d.Price
                }).ToList() ?? new List<GhnOrderItem>
                {
                    new GhnOrderItem { Name = "Do choi tre em", Quantity = 1, Price = (int)order.TotalPrice }
                };

                var body = new GhnCreateOrderRequest
                {
                    ClientOrderCode = order.Id != Guid.Empty ? order.Id.ToString() : null,
                    PaymentTypeId = 2,
                    Note = "Don hang Cua hang do choi",
                    RequiredNote = "KHONGCHOXEMHANG",
                    FromName = "ToysWorld",
                    FromPhone = "0387261918",
                    FromAddress = "Phuong Minh Khai, Quan Bac Tu Liem, Ha Noi",
                    FromDistrictId = _fromDistrictId,
                    FromWardCode = _fromWardCode,
                    ToName = !string.IsNullOrEmpty(recipientName) ? recipientName : "Khach hang",
                    ToPhone = !string.IsNullOrEmpty(recipientPhone) ? recipientPhone : "0987654321",
                    ToAddress = toAddress,
                    ToWardCode = toWardCode,
                    ToDistrictId = toDistrictId,
                    CodAmount = (int)codAmount,
                    Content = "Do choi tre em",
                    Weight = 500,
                    Length = 20,
                    Width = 15,
                    Height = 10,
                    ServiceTypeId = 2,
                    Items = items
                };

                var response = await SendRequestAsync(HttpMethod.Post, "v2/shipping-order/create", body, includeShopId: true);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GhnResponse<GhnCreateOrderData>>(json, _jsonOptions);
                if (result != null && result.Code == 200 && result.Data != null)
                {
                    return result.Data;
                }

                // Nếu đang ở môi trường Dev và gọi Sandbox bị 401 -> Sinh mã giả lập DEV_ORDER để test Webhook
                if (_baseUrl.Contains("dev-online-gateway"))
                {
                    var devCode = $"DEV_{DateTime.UtcNow.Ticks.ToString()[^8..]}";
                    Console.WriteLine($"🧪 Dev Sandbox Mode: Created test GhnOrderCode '{devCode}' for local Webhook testing.");
                    return new GhnCreateOrderData
                    {
                        OrderCode = devCode,
                        TotalFee = 30000,
                        ExpectedDeliveryTime = DateTime.UtcNow.AddDays(2).ToString("o")
                    };
                }

                Console.WriteLine($"GHN CreateShippingOrder error response: {json}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating GHN shipping order: {ex.Message}");
                return null;
            }
        }

        public async Task ProcessWebhookAsync(GhnWebhookDto dto)
        {
            if (dto == null || (string.IsNullOrEmpty(dto.OrderCode) && string.IsNullOrEmpty(dto.ClientOrderCode)))
                return;

            var orderCode = dto.OrderCode?.Trim();
            var clientOrderCode = dto.ClientOrderCode?.Trim();

            Console.WriteLine($"🔔 Received GHN Webhook | OrderCode: '{orderCode}', ClientOrderCode: '{clientOrderCode}', Status: '{dto.Status}', Type: '{dto.Type}'");

            var order = await _unitOfWork.orderRepo.GetAll()
                .FirstOrDefaultAsync(o => !o.IsDeleted && 
                    ((!string.IsNullOrEmpty(orderCode) && o.GhnOrderCode == orderCode) || 
                     (!string.IsNullOrEmpty(clientOrderCode) && o.Id.ToString() == clientOrderCode)));

            if (order == null)
            {
                Console.WriteLine($"⚠️ Order with GhnOrderCode '{dto.OrderCode}' or ClientOrderCode '{dto.ClientOrderCode}' not found in database.");
                return;
            }

            var statusLower = dto.Status?.ToLower()?.Trim() ?? "";
            order.GhnStatus = statusLower;

            switch (statusLower)
            {
                case "ready_to_pick":
                case "picking":
                case "money_collect_picking":
                case "picked":
                    order.OrderStatus = TrangThaiDonHang.Đã_xác_nhận;
                    break;

                case "storing":
                case "transporting":
                case "sorting":
                case "delivering":
                case "money_collect_delivering":
                    order.OrderStatus = TrangThaiDonHang.Đang_giao_hàng;
                    break;

                case "delivered":
                    order.OrderStatus = TrangThaiDonHang.Đã_giao_hàng;
                    break;

                case "cancel":
                case "delivery_fail":
                case "waiting_to_return":
                case "return":
                case "return_transporting":
                case "return_sorting":
                case "returning":
                case "return_fail":
                case "returned":
                case "exception":
                case "damage":
                case "lost":
                    order.OrderStatus = TrangThaiDonHang.Đã_hủy;
                    break;

                default:
                    Console.WriteLine($"⚠️ Unmapped GHN status '{dto.Status}' for order {order.Id}");
                    break;
            }

            await _unitOfWork.orderRepo.Update(order);
            await _unitOfWork.CompleteAsync();
            Console.WriteLine($"✅ Order {order.Id} status updated to '{order.OrderStatus}' via GHN Webhook (GHN Status: '{dto.Status}').");

            if (_orderNotifier != null)
            {
                await _orderNotifier.NotifyOrderStatusChangedAsync(order.Id, order.GhnOrderCode, (int)order.OrderStatus, order.OrderStatus.ToString(), dto.Status);
            }
        }
    }
}
