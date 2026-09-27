using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using static Core.Entities.Enum;

namespace DataAccess.Repo
{
    public class StaffTypePermissionRepo : BaseRepo<StaffTypePermission>
    {
        public StaffTypePermissionRepo(DatabaseContext context) : base(context) { }

        public IQueryable<StaffTypePermission> GetByStaffType(StaffType staffType)
        {
            return Context.Set<StaffTypePermission>()
                .Include(x => x.Permission)
                .Where(x => x.StaffType == staffType);
        }
    }
}
