using Core.Common;
using Core.Entities;
using Core.Entities.Identity;
using DataAccess.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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
        // PRODUCT / NEWS / PROMOTION
        // =========================
        public DbSet<Product> Products { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<News> News { get; set; }

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

            // Tự động áp dụng các IEntityTypeConfiguration nếu project có
            builder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly());

            // ApplicationUser <-> UserRole <-> ApplicationRole
            builder.ConfigUserRole();

            // UserPermission + StaffTypePermission
            builder.ConfigurePermissionRelations();

            // NEWS
            builder.ConfigureNew();

            // PROMOTION
            builder.ConfigurePromotion();
        }

        // =========================
        // SAVE CHANGES
        // =========================
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await GenerateSlugsAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            GenerateSlugsSync();
            return base.SaveChanges();
        }

        // =========================
        // SLUG - ASYNC
        // =========================
        private async Task GenerateSlugsAsync()
        {
            // NEWS
            foreach (var e in ChangeTracker.Entries<News>())
            {
                if (e.State == EntityState.Added ||
                    (e.State == EntityState.Modified &&
                     e.Property(p => p.Title).IsModified))
                {
                    var baseText = e.Entity.Title ?? string.Empty;

                    e.Entity.Slug =
                        await MakeUniqueAsync<News>(
                            baseText,
                            e.Entity.Id);
                }
            }

            // PROMOTION
            foreach (var e in ChangeTracker.Entries<Promotion>())
            {
                if (e.State == EntityState.Added ||
                    (e.State == EntityState.Modified &&
                     e.Property(p => p.Title).IsModified))
                {
                    var baseText = e.Entity.Title ?? string.Empty;

                    e.Entity.Slug =
                        await MakeUniqueAsync<Promotion>(
                            baseText,
                            e.Entity.Id);
                }
            }
        }

        // =========================
        // SLUG - SYNC
        // =========================
        private void GenerateSlugsSync()
        {
            // NEWS
            foreach (var e in ChangeTracker.Entries<News>())
            {
                if (e.State == EntityState.Added ||
                    (e.State == EntityState.Modified &&
                     e.Property(p => p.Title).IsModified))
                {
                    var baseText = e.Entity.Title ?? string.Empty;

                    e.Entity.Slug =
                        MakeUniqueSync<News>(
                            baseText,
                            e.Entity.Id);
                }
            }

            // PROMOTION
            foreach (var e in ChangeTracker.Entries<Promotion>())
            {
                if (e.State == EntityState.Added ||
                    (e.State == EntityState.Modified &&
                     e.Property(p => p.Title).IsModified))
                {
                    var baseText = e.Entity.Title ?? string.Empty;

                    e.Entity.Slug =
                        MakeUniqueSync<Promotion>(
                            baseText,
                            e.Entity.Id);
                }
            }
        }

        // =========================
        // UNIQUE SLUG - ASYNC
        // =========================
        private async Task<string> MakeUniqueAsync<T>(
            string baseText,
            Guid currentId)
            where T : class
        {
            var baseSlug = SlugHelper.ToSlug(baseText);

            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "item";

            var slug = baseSlug;
            var i = 2;

            while (await SlugExistsAsync<T>(slug, currentId))
            {
                slug = $"{baseSlug}-{i++}";
            }

            return slug;
        }

        // =========================
        // UNIQUE SLUG - SYNC
        // =========================
        private string MakeUniqueSync<T>(
            string baseText,
            Guid currentId)
            where T : class
        {
            var baseSlug = SlugHelper.ToSlug(baseText);

            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "item";

            var slug = baseSlug;
            var i = 2;

            while (SlugExistsSync<T>(slug, currentId))
            {
                slug = $"{baseSlug}-{i++}";
            }

            return slug;
        }

        // =========================
        // CHECK SLUG - ASYNC
        // =========================
        private Task<bool> SlugExistsAsync<T>(
            string slug,
            Guid currentId)
            where T : class
        {
            if (typeof(T) == typeof(News))
            {
                return News.AnyAsync(
                    x => x.Slug == slug &&
                         x.Id != currentId);
            }

            if (typeof(T) == typeof(Promotion))
            {
                return Promotions.AnyAsync(
                    x => x.Slug == slug &&
                         x.Id != currentId);
            }

            return Task.FromResult(false);
        }

        // =========================
        // CHECK SLUG - SYNC
        // =========================
        private bool SlugExistsSync<T>(
            string slug,
            Guid currentId)
            where T : class
        {
            if (typeof(T) == typeof(News))
            {
                return News.Any(
                    x => x.Slug == slug &&
                         x.Id != currentId);
            }

            if (typeof(T) == typeof(Promotion))
            {
                return Promotions.Any(
                    x => x.Slug == slug &&
                         x.Id != currentId);
            }

            return false;
        }
    }
}