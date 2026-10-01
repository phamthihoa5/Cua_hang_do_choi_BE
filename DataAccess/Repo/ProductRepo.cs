using Core.Entities;
using DataAccess.Repo.Common;

namespace DataAccess.Repo
{
    public class ProductRepo : BaseRepo<Product>
    {
        public ProductRepo(DatabaseContext context)
            : base(context)
        {
        }

        public IQueryable<Product> GetById(Guid id, bool isDeleted = false)
        {
            return Context.Set<Product>()
                .Where(x => x.Id == id && x.IsDeleted == isDeleted);
        }
    }
}