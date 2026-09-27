using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class OrderDetailConfiguration
    {
        public static void ConfigureOrderDetail(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetails>(entity =>
            {
                entity.HasOne(o => o.Order)
                    .WithMany(od => od.OrderDetail)
                    .HasForeignKey(o => o.IdOrder)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Product)
                    .WithMany(od => od.OrderDetails)
                    .HasForeignKey(p => p.IdProduct)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
