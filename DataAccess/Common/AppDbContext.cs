using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Common;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<WarehouseDetail> WarehouseDetails => Set<WarehouseDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
