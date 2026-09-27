using Application.Model.OrderDetail;
using Application.Model.User;
using System;
using System.Collections.Generic;
using static Core.Entities.Enum;

namespace Application.Model.Order
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }

        public UserViewDto User { get; set; }

        public DateTime OrderDate { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int? ProvinceId { get; set; }

        public int? DistrictId { get; set; }

        public string? WardCode { get; set; }

        public decimal ShippingFee { get; set; }

        public string? GhnOrderCode { get; set; }

        public string? GhnStatus { get; set; }

        public decimal TotalPrice { get; set; }

        public TrangThaiDonHang OrderStatus { get; set; }

        public string? TransactionCode { get; set; }

        public string? PaymentUrl { get; set; }

        public ICollection<OrderDetailResponseDto> OrderDetails { get; set; } = new List<OrderDetailResponseDto>();
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ListOrderDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public ICollection<OrderResponseDto> Orders { get; set; } = new List<OrderResponseDto>();
    }

    public class CreateOrderDto
    {
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string? BankCode { get; set; }
    }
}
