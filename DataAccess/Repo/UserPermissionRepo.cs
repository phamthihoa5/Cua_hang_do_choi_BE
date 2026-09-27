using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DataAccess.Repo
{
    public class UserPermissionRepo : BaseRepo<UserPermission>
    {
        public UserPermissionRepo(DatabaseContext context) : base(context) { }

        // Lấy tất cả quyền của 1 user
        public IQueryable<UserPermission> GetByUserId(Guid userId)
        {
            return Context.Set<UserPermission>()
                .Include(x => x.Permission)
                .Where(x => x.UserId == userId);
        }
    }
}
