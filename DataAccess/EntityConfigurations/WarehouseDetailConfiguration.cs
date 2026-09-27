using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EntityConfigurations
{
    public static class ProductWarehouseConfiguration
    {
        public static void ConfigureProductWarehouse(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WarehouseDetail>(entity =>
            {
                entity.HasOne(o => o.Warehouse)
                    .WithMany(od => od.WarehouseDetails)
                    .HasForeignKey(o => o.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Product)
                    .WithMany(od => od.Warehouses)
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
