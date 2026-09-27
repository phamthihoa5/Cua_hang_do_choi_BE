using Application.IService.User;
using Application.Interface;
using Application.Model.Order;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.AppService.User
{
    public class PaymentService : IPaymentService
    {
        private readonly IVnPayService _vnpay;
        private readonly IUnitOfWork _uow;

        public PaymentService(IVnPayService vnpay, IUnitOfWork uow)
        {
            _vnpay = vnpay;
            _uow = uow;
        }

        // 🟢 1️⃣ Tạo thanh toán VNPay
        public async Task<object> CreatePaymentAsync(CreateOrderDto dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Số tiền không hợp lệ.");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TransactionCode = $"ORDER{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalPrice = dto.Amount,
                OrderStatus = Core.Entities.Enum.TrangThaiDonHang.Chờ_xử_lý,
                CreatedOn = DateTime.UtcNow
            };

            await _uow.orderRepo.Add(order);
            await _uow.CompleteAsync();

            // Tạo URL VNPay
            var paymentUrl = await _vnpay.CreatePaymentUrlAsync(order.TransactionCode, dto.Amount, $"Thanh toán đơn {order.TransactionCode}");
            order.PaymentUrl = paymentUrl;

            await _uow.orderRepo.Update(order);
            await _uow.CompleteAsync();

            return new
            {
                orderId = order.Id,
                transactionCode = order.TransactionCode,
                paymentUrl
            };
        }

        // 🟢 2️⃣ Xử lý khi người dùng được redirect về (return URL)
        public async Task<string> HandleReturnUrlAsync(IDictionary<string, string> queryParams)
        {
            if (!await _vnpay.ValidateCallbackAsync(queryParams))
                return "invalid_signature";

            queryParams.TryGetValue("vnp_ResponseCode", out var responseCode);
            queryParams.TryGetValue("vnp_TxnRef", out var txnRef);

            var order = await _uow.orderRepo.GetAll()
                .FirstOrDefaultAsync(o => o.TransactionCode == txnRef);

            if (order == null)
                return "not_found";

            order.PaymentStatus = responseCode == "00"
                ? Core.Entities.Enum.TrangThaiThanhToan.Đã_thanh_toán
                : Core.Entities.Enum.TrangThaiThanhToan.Thanh_toán_thất_bại;

            await _uow.orderRepo.Update(order);
            await _uow.CompleteAsync();

            return responseCode == "00" ? "success" : "failed";
        }

        // 🟢 3️⃣ Xử lý callback IPN từ VNPay
        public async Task<object> HandleIpnAsync(IDictionary<string, string> queryParams)
        {
            if (!await _vnpay.ValidateCallbackAsync(queryParams))
                return new { RspCode = "97", Message = "Invalid signature" };

            queryParams.TryGetValue("vnp_TxnRef", out var txnRef);
            queryParams.TryGetValue("vnp_ResponseCode", out var responseCode);

            var order = await _uow.orderRepo.GetAll()
                .FirstOrDefaultAsync(o => o.TransactionCode == txnRef);

            if (order == null)
                return new { RspCode = "01", Message = "Order not found" };

            order.PaymentStatus = responseCode == "00"
                ? Core.Entities.Enum.TrangThaiThanhToan.Đã_thanh_toán
                : Core.Entities.Enum.TrangThaiThanhToan.Thanh_toán_thất_bại;

            await _uow.orderRepo.Update(order);
            await _uow.CompleteAsync();

            return new { RspCode = "00", Message = "Success" };
        }
    }
}
