using Core.Entities.Identity;
using DataAccess.Repo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class UserRepo : BaseRepo<ApplicationUser>
    {
        public UserRepo(DatabaseContext context) : base(context)
        {
        }
        // Gọi theo Id: Mở Profile User
        public IQueryable<ApplicationUser> GetById(Guid id, bool isDeleted = false) =>
            Context.Set<ApplicationUser>().Where(x => x.Id == id && x.IsDeleted == isDeleted);

        // Lấy danh sách người dùng có vai trò "Customer"
        public IQueryable<ApplicationUser> GetCustomers()
        {
            return from user in Context.Users
                   join userRole in Context.UserRoles on user.Id equals userRole.UserId
                   join role in Context.Roles on userRole.RoleId equals role.Id
                   where role.Name == "customer"
                   select user;
        }
    }
}
