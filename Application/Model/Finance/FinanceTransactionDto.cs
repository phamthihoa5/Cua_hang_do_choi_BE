using System;
using static Core.Entities.Enum;

namespace Application.Model.Finance
{
    public class FinanceTransactionDto
    {
        public Guid OrderId { get; set; }

        public string? CustomerName { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public TrangThaiThanhToan PaymentStatus { get; set; }

        public TrangThaiDonHang OrderStatus { get; set; }

        public string? TransactionCode { get; set; }
    }
}