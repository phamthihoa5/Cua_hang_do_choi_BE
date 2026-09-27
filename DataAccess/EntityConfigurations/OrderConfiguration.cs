using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EntityConfigurations
{
    public static class OrderConfiguration
    {
        public static void ConfigureOrder(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(u => u.User)
                    .WithMany(o => o.Orders)
                    .HasForeignKey(u => u.IdUser)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
