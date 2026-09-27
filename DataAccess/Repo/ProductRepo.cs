using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class ProductRepo : BaseRepo<Product>
    {
        public ProductRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<Product> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Product>().Where(x => x.Id == id);

        // Gọi theo tên
        public IQueryable<Product> GetByName(string name, bool isDeleted = false) =>
            Context.Set<Product>().Where(x => x.ProductName == name && x.IsDeleted == isDeleted);

        // Gọi theo IdCategory
        public IQueryable<Product> GetByIdCategory(Guid idCategory, bool isDeleted = false) =>
            Context.Set<Product>().Where(x => x.IdCategory == idCategory && x.IsDeleted == isDeleted);

        // Gọi theo IdSupplier
        public IQueryable<Product> GetByIdSupplier(Guid idSupplier, bool isDeleted = false) =>
            Context.Set<Product>().Where(x => x.IdSupplier == idSupplier && x.IsDeleted == isDeleted);
        // phương thức để lấy được cả category và supplier
        public IQueryable<Product> GetAllWithInclude()
        {
            return Context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier);
        }

        // Gọi các sản phẩm có mã khuyến mãi đó
        public IQueryable<Product> GetProductsByPromotion(Guid promotionId, bool isDeleted = false) =>
            Context.Set<Product>().Where(p => p.IdPromotion == promotionId && p.IsDeleted == isDeleted);

    }
}
