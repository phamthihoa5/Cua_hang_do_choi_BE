using Application.Interface;
using Application.IService.User;
using Application.Model.Finance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class FinanceService : IFinanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FinanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FinanceSummaryDto> GetFinanceSummaryAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var ordersQuery = _unitOfWork.orderRepo.GetAll()
                .Where(o => !o.IsDeleted);

            var warehouseQuery = _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(w => !w.IsDeleted);

            if (fromDate.HasValue)
            {
                var startDate = DateTime.SpecifyKind(
                    fromDate.Value.Date,
                    DateTimeKind.Utc);

                ordersQuery = ordersQuery
                    .Where(o => o.OrderDate >= startDate);

                warehouseQuery = warehouseQuery
                    .Where(w => w.Warehouse.DateEntered >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDate = DateTime.SpecifyKind(
                    toDate.Value.Date,
                    DateTimeKind.Utc)
                    .AddDays(1);

                ordersQuery = ordersQuery
                    .Where(o => o.OrderDate < endDate);

                warehouseQuery = warehouseQuery
                    .Where(w => w.Warehouse.DateEntered < endDate);
            }

            var totalRevenue = await ordersQuery
                .Where(o =>
                    o.OrderStatus != TrangThaiDonHang.Đã_hủy &&
                    o.PaymentStatus == TrangThaiThanhToan.Đã_thanh_toán)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var totalCost = await warehouseQuery
                .SumAsync(w => (decimal?)(w.ImportPrice * w.Quantity)) ?? 0;

            var totalOrders = await ordersQuery.CountAsync();

            var paidOrders = await ordersQuery
                .CountAsync(o =>
                    o.PaymentStatus ==
                    TrangThaiThanhToan.Đã_thanh_toán);

            var unpaidOrders = await ordersQuery
                .CountAsync(o =>
                    o.PaymentStatus ==
                    TrangThaiThanhToan.Chưa_thanh_toán);

            var cancelledOrders = await ordersQuery
                .CountAsync(o =>
                    o.OrderStatus ==
                    TrangThaiDonHang.Đã_hủy);

            return new FinanceSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                TotalProfit = totalRevenue - totalCost,
                TotalOrders = totalOrders,
                PaidOrders = paidOrders,
                UnpaidOrders = unpaidOrders,
                CancelledOrders = cancelledOrders
            };
        }

        public async Task<List<FinanceTransactionDto>> GetTransactionsAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _unitOfWork.orderRepo.GetAll()
                .Include(o => o.User)
                .Where(o => !o.IsDeleted);

            if (fromDate.HasValue)
            {
                var startDate = DateTime.SpecifyKind(
                    fromDate.Value.Date,
                    DateTimeKind.Utc);

                query = query.Where(o =>
                    o.OrderDate >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDate = DateTime.SpecifyKind(
                    toDate.Value.Date,
                    DateTimeKind.Utc)
                    .AddDays(1);

                query = query.Where(o =>
                    o.OrderDate < endDate);
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new FinanceTransactionDto
                {
                    OrderId = o.Id,
                    CustomerName = o.User.FullName,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalPrice,
                    PaymentStatus = o.PaymentStatus,
                    OrderStatus = o.OrderStatus,
                    TransactionCode = o.TransactionCode
                })
                .ToListAsync();
        }
    }
}