using Core.Common;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using static Core.Entities.Enum;

namespace Core.Entities
{
    [Table("Order")]
    public class Order : BaseEntity, IAuditedEntity
    {
        public Guid IdUser { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public TrangThaiDonHang OrderStatus { get; set; } = TrangThaiDonHang.Chờ_xử_lý;
        public TrangThaiThanhToan PaymentStatus { get; set; } = TrangThaiThanhToan.Chưa_thanh_toán;

        // GHN Shipping Properties
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public string? WardCode { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public string? GhnOrderCode { get; set; }
        public string? GhnStatus { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        public string? TransactionCode { get; set; }   // Ma giao dich VNPay
        public string? PaymentUrl { get; set; }        // Link thanh toan VNPay

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public ApplicationUser User { get; set; }
        public virtual ICollection<OrderDetails> OrderDetail { get; set; }
    }
}
