using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class CartConfiguration
    {
        public static void ConfigureCart(this ModelBuilder modelBuilder)
        {
            // Quan hệ giữa Cart và User (Một-nhiều)
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(u => u.User)
                    .WithMany(c => c.Carts)
                    .HasForeignKey(u => u.IdUser)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Quan hệ giữa Cart và Product (Một-một hoặc Một-nhiều)
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(p => p.Product) // Nếu Cart chỉ chứa một sản phẩm
                    .WithMany(c => c.Carts)  // Nếu sản phẩm có thể nằm trong nhiều Cart
                    .HasForeignKey(p => p.IdProduct)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
