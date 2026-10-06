using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class ProductConfiguration
    {
        public static void ConfigureProduct(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.HasMany(p => p.Carts)
                       .WithOne(c => c.Product)
                       .HasForeignKey(c => c.IdProduct)
                       .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.OrderDetails)
                       .WithOne(od => od.Product)
                       .HasForeignKey(od => od.IdProduct)
                       .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Warehouses)
                       .WithOne(w => w.Product)
                       .HasForeignKey(w => w.ProductId)
                       .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Supplier)
                       .WithMany(s => s.Products)
                       .HasForeignKey(p => p.IdSupplier)
                       .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.IdCategory)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Promotion)
                        .WithMany(pr => pr.Products)
                        .HasForeignKey(p => p.IdPromotion)
                        .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
