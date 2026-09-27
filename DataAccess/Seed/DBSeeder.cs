using Core.Entities;
using Core.Entities.Identity;
using DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using static Core.Entities.Enum;

public static class DbSeeder
{
    public static async Task SeedAsync(
        DatabaseContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        try
            {
                await context.Database.MigrateAsync();
            }
            catch
            {
                // Tables already exist or migration skipped
            }

        // ========== 1️⃣ ROLE ==========
        await EnsureRoleAsync(roleManager, RoleName.ManagerId, RoleName.Manager, RoleName.MANAGER, 1);
        await EnsureRoleAsync(roleManager, RoleName.StaffId, RoleName.Staff, RoleName.STAFF, 2);
        await EnsureRoleAsync(roleManager, RoleName.CustomerId, RoleName.Customer, RoleName.CUSTOMER, 3);

        // ========== 2️⃣ ADMIN (FULL QUYỀN) ==========
        var manager = await EnsureManagerAsync(userManager);
        if (!await userManager.IsInRoleAsync(manager, RoleName.Manager))
            await userManager.AddToRoleAsync(manager, RoleName.Manager);

        // ========== 3️⃣ PHÂN QUYỀN ADMIN ==========
        await EnsureManagerPermissionsAsync(context, manager.Id);

        // ========== 4️⃣ PHÂN QUYỀN CHO NHÂN VIÊN ==========
        await EnsureDefaultStaffTypePermissionsAsync(context);
    }

    // ============================== 
    // ROLE 
    // ============================== 
    private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleMgr, Guid id, string name, string normalized, int priority)
    {
        var role = await roleMgr.FindByIdAsync(id.ToString());
        if (role == null)
        {
            var newRole = new ApplicationRole
            {
                Id = id,
                Name = name,
                NormalizedName = normalized,
                Priority = priority
            };
            var res = await roleMgr.CreateAsync(newRole);
            if (!res.Succeeded)
                throw new Exception("Seed role failed: " + string.Join("; ", res.Errors.Select(e => e.Description)));
        }
    }

    // ============================== 
    // ADMIN (MANAGER) 
    // ============================== 
    private static async Task<ApplicationUser> EnsureManagerAsync(UserManager<ApplicationUser> userMgr)
    {
        const string adminUserName = "admin";
        const string adminEmail = "admin@yourapp.local";
        const string adminPassword = "Admin@12345";

        var manager = await userMgr.FindByNameAsync(adminUserName);
        if (manager == null)
        {
            manager = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminUserName,
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpperInvariant(),
                EmailConfirmed = true,
                FullName = "Quản trị viên hệ thống",
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            };

            var res = await userMgr.CreateAsync(manager, adminPassword);
            if (!res.Succeeded)
                throw new Exception("Seed manager failed: " + string.Join("; ", res.Errors.Select(e => e.Description)));
        }
        return manager;
    }

    // ============================== 
    // PHÂN QUYỀN ADMIN FULL QUYỀN 
    // ============================== 
    private static async Task EnsureManagerPermissionsAsync(DatabaseContext ctx, Guid managerId)
    {
        var allPermissionIds = await ctx.Permissions.AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync();

        var existing = await ctx.UserPermissions
            .Where(up => up.UserId == managerId)
            .Select(up => up.PermissionId)
            .ToListAsync();

        var toAdd = allPermissionIds
            .Where(pid => !existing.Contains(pid))
            .Select(pid => new UserPermission
            {
                Id = Guid.NewGuid(),
                UserId = managerId,
                PermissionId = pid,
                IsGranted = true,
                GrantedOn = DateTime.UtcNow,
                GrantedById = managerId
            })
            .ToList();

        if (toAdd.Any())
        {
            ctx.UserPermissions.AddRange(toAdd);
            await ctx.SaveChangesAsync();
        }
    }

    // ============================== 
    // PHÂN QUYỀN NHÂN VIÊN (GIỮ LẠI HÀM NÀY) 
    // ============================== 
    public static async Task EnsureDefaultStaffTypePermissionsAsync(DatabaseContext ctx)
    {
        var staffTypePermissions = new Dictionary<StaffType, string[]>
        {
            [StaffType.Sales] = new[]
            {
                "PRODUCT_VIEW", "PRODUCT_CREATE", "PRODUCT_UPDATE",
                "CATEGORY_VIEW",
                "ORDER_VIEW", "ORDER_CREATE", "ORDER_UPDATE",
                "PROMOTION_VIEW", "PROMOTION_CREATE",
                "NEWS_VIEW", "NEWS_CREATE"
            },
            [StaffType.Warehouse] = new[]
            {
                "PRODUCT_VIEW", "PRODUCT_UPDATE",
                "CATEGORY_VIEW",
                "SUPPLIER_VIEW", "SUPPLIER_UPDATE",
                "WAREHOUSE_VIEW", "WAREHOUSE_UPDATE", "WAREHOUSE_CREATE"
            }
        };

        var allPermissions = await ctx.Permissions.AsNoTracking().ToListAsync();
        var existing = await ctx.StaffTypePermissions.AsNoTracking().ToListAsync();

        var toAdd = new List<StaffTypePermission>();

        foreach (var (staffType, codes) in staffTypePermissions)
        {
            foreach (var code in codes)
            {
                var perm = allPermissions.FirstOrDefault(p => p.Code == code);
                if (perm != null && !existing.Any(x => x.StaffType == staffType && x.PermissionId == perm.Id))
                {
                    toAdd.Add(new StaffTypePermission
                    {
                        Id = Guid.NewGuid(),
                        StaffType = staffType,
                        PermissionId = perm.Id,
                        IsGranted = true
                    });
                }
            }
        }

        if (toAdd.Any())
        {
            ctx.StaffTypePermissions.AddRange(toAdd);
            await ctx.SaveChangesAsync();
        }
    }
}
