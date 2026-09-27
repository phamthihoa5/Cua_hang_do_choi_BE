using Core.Entities;
using DataAccess.Repo.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class OrderDetailsRepo : BaseRepo<OrderDetails>
    {
        public OrderDetailsRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<OrderDetails> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<OrderDetails>().Where(x => x.Id == id && x.IsDeleted == isDeleted)
             .Include(o => o.Product);

        // Gọi theo OrderId
        public IQueryable<OrderDetails> GetByOrderId(Guid orderId, bool isDeleted = false) =>
            Context.Set<OrderDetails>().Where(x => x.IdOrder == orderId && x.IsDeleted == isDeleted)
             .Include(o => o.Product);

        // Gọi theo ProductId
        public IQueryable<OrderDetails> GetByProductId(Guid productId, bool isDeleted = false) =>
            Context.Set<OrderDetails>().Where(x => x.IdProduct == productId && x.IsDeleted == isDeleted)
            .Include(o => o.Product);
    }
}
