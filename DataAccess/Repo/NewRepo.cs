using Core.Entities;
using DataAccess.Repo.Common;
using System;
using System.Linq;

namespace DataAccess.Repo
{
    public class NewRepo : BaseRepo<News>
    {
        public NewRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<News> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<News>().Where(x => x.Id == id && x.IsDeleted == isDeleted);

        // Gọi theo tiêu đề
        public IQueryable<News> GetByTitle(string title, bool isDeleted = false) =>
            Context.Set<News>().Where(x => x.Title == title && x.IsDeleted == isDeleted);

        // Gọi theo slug
        public IQueryable<News> GetBySlug(string slug, bool isDeleted = false) =>
            Context.Set<News>().Where(x => x.Slug == slug && x.IsDeleted == isDeleted);

        // Gọi các bản tin đã duyệt
        public IQueryable<News> GetApproved(bool isDeleted = false) =>
            Context.Set<News>().Where(x => x.IsApproved && x.IsDeleted == isDeleted);
    }
}
