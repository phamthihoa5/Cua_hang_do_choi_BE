using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class WarehouseConfiguration
    {
        public static void ConfigureWarehouse(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Warehouse>(entity =>
            {
                

                //entity.HasOne(w => w.Suppliers)
                //       .WithMany(s => s.Warehouses)
                //      // .HasForeignKey(w => w.IdSupplier)
                //       .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
