using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EntityConfigurations
{
    public static class PermissionConfiguration
    {
        public static void ConfigurePermissionRelations(this ModelBuilder modelBuilder)
        {
            // ==============================
            // 1️⃣ UserPermission Configuration
            // ==============================
            modelBuilder.Entity<UserPermission>(entity =>
            {

                // UserPermission → ApplicationUser
                entity.HasOne(x => x.User)
                      .WithMany(u => u.UserPermissions)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // UserPermission → Permission
                entity.HasOne(x => x.Permission)
                      .WithMany(p => p.UserPermissions)
                      .HasForeignKey(x => x.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // UserPermission → GrantedBy (người cấp quyền)
                entity.HasOne(x => x.GrantedBy)
                      .WithMany()
                      .HasForeignKey(x => x.GrantedById)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ==============================
            // 2️⃣ StaffTypePermission Configuration
            // ==============================
            modelBuilder.Entity<StaffTypePermission>(entity =>
            {


                // Quan hệ: StaffTypePermission → Permission
                entity.HasOne(x => x.Permission)
                      .WithMany(p => p.StaffTypePermissions)
                      .HasForeignKey(x => x.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
