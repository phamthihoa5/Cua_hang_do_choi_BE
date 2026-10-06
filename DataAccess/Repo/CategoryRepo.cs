using Core.Entities;
using DataAccess.Repo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class CategoryRepo : BaseRepo<Category>
    {
        public CategoryRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<Category> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Category>().Where(x => x.Id == id);

        // Gọi theo tên
        public IQueryable<Category> GetByName(string name, bool isDeleted = false) =>
            Context.Set<Category>().Where(x => x.CategoryName == name && x.IsDeleted == isDeleted);





    }
}
