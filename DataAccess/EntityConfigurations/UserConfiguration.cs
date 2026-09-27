using Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigurations
{

    public static class UserConfiguration
    {
        public static void ConfigUserRole(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.RoleId);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Roles)
                    .HasForeignKey(e => e.UserId);
            });
        }
    }
}
