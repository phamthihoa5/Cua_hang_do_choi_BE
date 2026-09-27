using Application.Model.Cart;
using System.Collections.Generic;

namespace Application.Model.Order
{
    public class OrderRequestDto
    {
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public string? WardCode { get; set; }
        public decimal? ShippingFee { get; set; }
        public string? RecipientName { get; set; }
        public List<CartRequestDto>? Products { get; set; } = new();
    }

    public class UpdateOrderDto
    {
        public Core.Entities.Enum.TrangThaiDonHang OrderStatus { get; set; }
    }
}
