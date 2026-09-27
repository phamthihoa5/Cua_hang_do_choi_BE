using Core.Entities;
using DataAccess.Repo.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static Core.Entities.Enum;

namespace DataAccess.Repo
{
    public class PermissionRepo : BaseRepo<Permission>
    {
        public PermissionRepo(DatabaseContext context) : base(context) { }

        // === Lấy toàn bộ quyền hệ thống ===
        public IQueryable<Permission> GetAllPermissions(bool asNoTracking = true)
        {
            var query = Context.Set<Permission>().AsQueryable();
            if (asNoTracking) query = query.AsNoTracking();
            return query.OrderBy(x => x.Code);
        }

        // === Lấy quyền theo mã Code ===
        public IQueryable<Permission> GetByCode(string code, bool asNoTracking = true)
        {
            var query = Context.Set<Permission>().Where(x => x.Code == code);
            if (asNoTracking) query = query.AsNoTracking();
            return query;
        }

        // === Lấy quyền của 1 user ===
        public IQueryable<UserPermission> GetUserPermissions(Guid userId, bool asNoTracking = true)
        {
            var query = Context.Set<UserPermission>()
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId);

            if (asNoTracking) query = query.AsNoTracking();
            return query;
        }

        // === Lấy quyền mặc định theo loại nhân viên ===
        public IQueryable<StaffTypePermission> GetByStaffType(StaffType staffType, bool asNoTracking = true)
        {
            var query = Context.Set<StaffTypePermission>()
                .Include(x => x.Permission)
                .Where(x => x.StaffType == staffType);

            if (asNoTracking) query = query.AsNoTracking();
            return query;
        }

        // === Lấy danh sách PermissionId mặc định theo StaffType ===
        public IQueryable<Guid> GetDefaultPermissionIds(StaffType staffType)
        {
            return Context.Set<StaffTypePermission>()
                .Where(x => x.StaffType == staffType && x.IsGranted)
                .Select(x => x.PermissionId);
        }

        // === Xóa toàn bộ quyền của user ===
        public IQueryable<UserPermission> GetUserPermissionForDelete(Guid userId)
        {
            return Context.Set<UserPermission>()
                .Where(up => up.UserId == userId);
        }
    }
}
