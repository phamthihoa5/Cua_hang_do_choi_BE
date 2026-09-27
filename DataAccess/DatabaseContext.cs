using System.Reflection;
using Core.Common; // SlugHelper.ToSlug
using Core.Entities;
using Core.Entities.Identity;
using DataAccess.EntityConfigurations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Services.ClaimService;

namespace DataAccess;

public class DatabaseContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    private readonly IClaimService? _claimService;

    public DatabaseContext(DbContextOptions options, IClaimService claimService) : base(options)
    {
        _claimService = claimService;
    }

    public DbSet<Cart> Carts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<WarehouseDetail> WarehouseDetails { get; set; }
    public DbSet<StaffTypePermission> StaffTypePermissions { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.ConfigureProduct();
        builder.ConfigureCategory();
        builder.ConfigureOrder();
        builder.ConfigureOrderDetail();
        builder.ConfigureSupplier();
        builder.ConfigureWarehouse();
        builder.ConfigurePermissionRelations();
        builder.ConfigureNew();
        builder.ConfigurePromotion();
        builder.ConfigureCart();
        builder.ConfigUserRole();
        builder.ConfigureProductWarehouse();
    }

    // =========================
    // SAVE CHANGES
    // =========================
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await SaveChangesAsync(null, cancellationToken);

    public async Task<int> SaveChangesAsync(string? id = null, CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        await GenerateSlugsAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
        => SaveChanges(null);

    public int SaveChanges(string? id = null)
    {
        ApplyAudit();
        GenerateSlugsSync();
        return base.SaveChanges();
    }

    // =========================
    // AUDIT
    // =========================
    private void ApplyAudit()
    {
        var userName = _claimService?.GetName() ?? "system";
        var utcNow = DateTime.UtcNow;

        // 🧩 Thêm log để kiểm tra giá trị thực tế
        Console.WriteLine($"[AUDIT DEBUG] Current UserName from ClaimService: '{userName}'");

        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userName;
                entry.Entity.CreatedOn = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.CreatedBy).IsModified = false;
                entry.Property(e => e.CreatedOn).IsModified = false;

                entry.Entity.UpdatedBy = userName;
                entry.Entity.UpdatedOn = utcNow;
            }
        }
    }




    // =========================
    // SLUG LOGIC (ASYNC)
    // =========================
    private async Task GenerateSlugsAsync()
    {
        // Product
        foreach (var e in ChangeTracker.Entries<Product>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.ProductName).IsModified))
            {
                var baseText = e.Entity.ProductName ?? string.Empty;
                e.Entity.Slug = await MakeUniqueAsync<Product>(baseText, e.Entity.Id);
            }
        }

        // Category
        foreach (var e in ChangeTracker.Entries<Category>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified &&
                 (e.Property(p => p.CategoryName).IsModified || e.Property(p => p.ParentId).IsModified)))
            {
                var baseText = e.Entity.CategoryName ?? string.Empty;
                var slug = SlugHelper.ToSlug(baseText);

                if (e.Entity.ParentId != null)
                {
                    var parentSlug = await Categories
                        .Where(c => c.Id == e.Entity.ParentId)
                        .Select(c => c.Slug)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(parentSlug))
                        slug = $"{parentSlug}/{slug}";
                }

                e.Entity.Slug = await MakeUniqueAsync<Category>(slug, e.Entity.Id);
            }
        }

        // News
        foreach (var e in ChangeTracker.Entries<News>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.Title).IsModified))
            {
                var baseText = e.Entity.Title ?? string.Empty;
                e.Entity.Slug = await MakeUniqueAsync<News>(baseText, e.Entity.Id);
            }
        }

        // Promotion
        foreach (var e in ChangeTracker.Entries<Promotion>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.Title).IsModified))
            {
                var baseText = e.Entity.Title ?? string.Empty;
                e.Entity.Slug = await MakeUniqueAsync<Promotion>(baseText, e.Entity.Id);
            }
        }
    }

    // =========================
    // SLUG LOGIC (SYNC)
    // =========================
    private void GenerateSlugsSync()
    {
        // Product
        foreach (var e in ChangeTracker.Entries<Product>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.ProductName).IsModified))
            {
                var baseText = e.Entity.ProductName ?? string.Empty;
                e.Entity.Slug = MakeUniqueSync<Product>(baseText, e.Entity.Id);
            }
        }

        // Category
        foreach (var e in ChangeTracker.Entries<Category>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified &&
                 (e.Property(p => p.CategoryName).IsModified || e.Property(p => p.ParentId).IsModified)))
            {
                var baseText = e.Entity.CategoryName ?? string.Empty;
                var slug = SlugHelper.ToSlug(baseText);

                if (e.Entity.ParentId != null)
                {
                    var parentSlug = Categories
                        .Where(c => c.Id == e.Entity.ParentId)
                        .Select(c => c.Slug)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(parentSlug))
                        slug = $"{parentSlug}/{slug}";
                }

                e.Entity.Slug = MakeUniqueSync<Category>(slug, e.Entity.Id);
            }
        }

        // News
        foreach (var e in ChangeTracker.Entries<News>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.Title).IsModified))
            {
                var baseText = e.Entity.Title ?? string.Empty;
                e.Entity.Slug = MakeUniqueSync<News>(baseText, e.Entity.Id);
            }
        }

        // Promotion
        foreach (var e in ChangeTracker.Entries<Promotion>())
        {
            if (e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(p => p.Title).IsModified))
            {
                var baseText = e.Entity.Title ?? string.Empty;
                e.Entity.Slug = MakeUniqueSync<Promotion>(baseText, e.Entity.Id);
            }
        }
    }

    // =========================
    // UNIQUE SLUG
    // =========================
    private async Task<string> MakeUniqueAsync<T>(string baseText, Guid currentId) where T : class
    {
        var baseSlug = SlugHelper.ToSlug(baseText);
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "item";

        var slug = baseSlug;
        var i = 2;

        while (await SlugExistsAsync<T>(slug, currentId))
            slug = $"{baseSlug}-{i++}";

        return slug;
    }

    private string MakeUniqueSync<T>(string baseText, Guid currentId) where T : class
    {
        var baseSlug = SlugHelper.ToSlug(baseText);
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "item";

        var slug = baseSlug;
        var i = 2;

        while (SlugExistsSync<T>(slug, currentId))
            slug = $"{baseSlug}-{i++}";

        return slug;
    }

    private Task<bool> SlugExistsAsync<T>(string slug, Guid currentId) where T : class
    {
        if (typeof(T) == typeof(Product))
            return Products.AnyAsync(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(Category))
            return Categories.AnyAsync(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(News))
            return News.AnyAsync(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(Promotion))
            return Promotions.AnyAsync(x => x.Slug == slug && x.Id != currentId);

        return Task.FromResult(false);
    }

    private bool SlugExistsSync<T>(string slug, Guid currentId) where T : class
    {
        if (typeof(T) == typeof(Product))
            return Products.Any(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(Category))
            return Categories.Any(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(News))
            return News.Any(x => x.Slug == slug && x.Id != currentId);
        if (typeof(T) == typeof(Promotion))
            return Promotions.Any(x => x.Slug == slug && x.Id != currentId);

        return false;
    }
}
