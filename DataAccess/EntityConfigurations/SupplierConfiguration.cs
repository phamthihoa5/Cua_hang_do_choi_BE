using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class SupplierConfiguration
    {
        public static void ConfigureSupplier(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasMany(e => e.Products)
                    .WithOne(s => s.Supplier)
                    .HasForeignKey(s => s.IdSupplier)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
