using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DataAccess.Repo
{
    public class WarehouseDetailRepo : BaseRepo<WarehouseDetail>
    {
        public WarehouseDetailRepo(DatabaseContext context) : base(context)
        {
        }

        // 🔹 Lấy theo ProductId
        public IQueryable<WarehouseDetail> GetByProductId(Guid productId, bool isDeleted = false) =>
            Context.Set<WarehouseDetail>()
                   .Where(x => x.ProductId == productId && x.IsDeleted == isDeleted)
                   .Include(x => x.Product)
                   .Include(x => x.Warehouse);

        // 🔹 Lấy theo WarehouseId
        public IQueryable<WarehouseDetail> GetByWarehouseId(Guid warehouseId, bool isDeleted = false) =>
            Context.Set<WarehouseDetail>()
                   .Where(x => x.WarehouseId == warehouseId && x.IsDeleted == isDeleted)
                   .Include(x => x.Product)
                   .Include(x => x.Warehouse);

        // 🔹 Lấy theo ProductId + WarehouseId (khóa kép)
        public IQueryable<WarehouseDetail> GetByCompositeKey(Guid productId, Guid warehouseId, bool isDeleted = false) =>
            Context.Set<WarehouseDetail>()
                   .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId && x.IsDeleted == isDeleted)
                   .Include(x => x.Product)
                   .Include(x => x.Warehouse);

        // 🔹 Lấy tất cả (có kèm Product & Warehouse)
        //public IQueryable<WarehouseDetail> GetAllWithInclude(bool includeDeleted = false)
        //{
        //    var query = Context.Set<WarehouseDetail>()
        //        .Include(x => x.Product)
        //        .Include(x => x.Warehouse);

        //    if (!includeDeleted)
        //        query = query.Where(x => !x.IsDeleted);

        //    return query;
        //}

        // 🔹 Lấy các bản ghi đã xóa (phục vụ khôi phục)
        public IQueryable<WarehouseDetail> GetDeleted()
        {
            return Context.Set<WarehouseDetail>()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x => x.IsDeleted);
        }
    }
}
