using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace DataAccess.Repo
{
    public class WarehouseRepo : BaseRepo<Warehouse>
    {
        public WarehouseRepo(DatabaseContext context) : base(context)
        {
        }

        // Gọi theo Id
        public IQueryable<Warehouse> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<Warehouse>().Where(x => x.Id == id);

        // Gọi theo trạng thái
        public IQueryable<Warehouse> GetByStatus(TrangThaiKhoHang status, bool isDeleted = false) =>
            Context.Set<Warehouse>().Where(x => x.Status == status && x.IsDeleted == isDeleted);

       

    }
}
