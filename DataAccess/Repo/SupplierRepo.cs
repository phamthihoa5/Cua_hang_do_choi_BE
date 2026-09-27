using Core.Entities;
using DataAccess.Repo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class SupplierRepo : BaseRepo<Supplier>
    {
        public SupplierRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<Supplier> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Supplier>().Where(x => x.Id == id);

        // Gọi theo tên
        public IQueryable<Supplier> GetByName(string name, bool isDeleted = false) =>
            Context.Set<Supplier>().Where(x => x.SupplierName == name && x.IsDeleted == isDeleted);
    }
}
