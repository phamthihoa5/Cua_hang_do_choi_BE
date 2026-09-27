using Application.Model.Ghn;
using Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IGhnService
    {
        Task<List<GhnProvinceDto>> GetProvincesAsync();
        Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId);
        Task<List<GhnWardDto>> GetWardsAsync(int districtId);
        Task<decimal> CalculateShippingFeeAsync(int toDistrictId, string toWardCode, int weight = 500);
        Task<GhnCreateOrderData> CreateShippingOrderAsync(Order order, int toDistrictId, string toWardCode, string toAddress, string recipientName, string recipientPhone, decimal codAmount = 0);
        Task ProcessWebhookAsync(GhnWebhookDto dto);
    }
}
