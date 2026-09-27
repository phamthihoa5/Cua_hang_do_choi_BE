using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace DataAccess.Repo
{
    public class OrderRepo : BaseRepo<Order>
    {
        public OrderRepo(DatabaseContext context) : base(context) { }

        public IQueryable<Order> GetById(Guid id) =>
            Context.Set<Order>().Where(x => x.Id == id)
            .Include(u => u.User);

        public IQueryable<Order> GetByUserId(Guid userId, bool isDeleted = false) =>
            Context.Set<Order>().Where(x => x.IdUser == userId && x.IsDeleted == isDeleted)
                                 .Include(u => u.User)
                                 .Include(o => o.OrderDetail)
                                 .ThenInclude(d => d.Product);

        public IQueryable<Order> GetByStatus(TrangThaiDonHang status, bool isDeleted = false) =>
            Context.Set<Order>().Where(x => x.OrderStatus == status && x.IsDeleted == isDeleted);

        public IQueryable<Order> GetAll() =>
            Context.Set<Order>()
                   .Include(u => u.User)
                   .Include(o => o.OrderDetail)
                       .ThenInclude(od => od.Product)
                   .OrderByDescending(o => o.OrderDate);

        // Bao gồm chi tiết và sản phẩm
        public IQueryable<Order> GetFullByUserId(Guid userId) =>
            Context.Orders
                   .Where(x => x.IdUser == userId)
                   .Include(u => u.User)
                   .Include(o => o.OrderDetail)
                       .ThenInclude(od => od.Product);

        public IQueryable<Order> GetFullById(Guid orderId) =>
            Context.Orders
                   .Where(x => x.Id == orderId)
                   .Include(o => o.OrderDetail)
                       .ThenInclude(od => od.Product);

        //Tìm đơn hàng theo mã giao dịch (VNPay)
        public async Task<Order?> FindByTransactionCodeAsync(string transactionCode)
        {
            return await Context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.TransactionCode == transactionCode);
        }
    }
}
