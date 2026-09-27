using Core.Entities;
using DataAccess.Repo.Common;
using System;
using System.Linq;

namespace DataAccess.Repo
{
    public class PromotionRepo : BaseRepo<Promotion>
    {
        public PromotionRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<Promotion> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Promotion>().Where(x => x.Id == id);

        // Gọi theo tiêu đề
        public IQueryable<Promotion> GetByTitle(string title, bool isDeleted = false) =>
            Context.Set<Promotion>().Where(x => x.Title == title && x.IsDeleted == isDeleted);
    }
}
