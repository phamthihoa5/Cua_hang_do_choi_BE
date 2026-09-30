using Core.Entities;
using Core.Entities.Identity;
using DataAccess.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DatabaseContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
            Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>,
            UserRole,
            Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>,
            Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>,
            Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        // =========================
        // PERMISSION
        // =========================
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<StaffTypePermission> StaffTypePermissions { get; set; }

        // =========================
        // MODEL CONFIGURATION
        // =========================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser <-> UserRole <-> ApplicationRole
            builder.ConfigUserRole();

            // UserPermission + StaffTypePermission
            builder.ConfigurePermissionRelations();
        }
    }
}