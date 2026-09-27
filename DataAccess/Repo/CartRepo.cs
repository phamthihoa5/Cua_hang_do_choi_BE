using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DataAccess.Repo
{
    public class CartRepo : BaseRepo<Cart>
    {
        public CartRepo(DatabaseContext context) : base(context) { }

        // Lấy 1 Cart theo Id (nếu cần)
        public IQueryable<Cart> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Cart>()
                .Where(x => x.Id == id && x.IsDeleted == isDeleted)
                .Include(x => x.Product);

        // Lấy toàn bộ giỏ hàng của 1 user (bao gồm Product và User)
        public IQueryable<Cart> GetByIdUser(Guid idUser, bool isDeleted = false) =>
            Context.Set<Cart>()
                .Where(x => x.IdUser == idUser && x.IsDeleted == isDeleted)
                .Include(x => x.Product)
                .Include(x => x.User);

        // Xóa sản phẩm trong giỏ hàng theo userId và productId
        public IQueryable<Cart> GetByUserAndProduct(Guid idUser, Guid idProduct, bool isDeleted = false) =>
            Context.Set<Cart>()
                .Where(x => x.IdUser == idUser && x.IdProduct == idProduct && x.IsDeleted == isDeleted)
                .Include(x => x.Product);
    }
}
