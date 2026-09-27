using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EntityConfigurations
{
    public static class CategoryConfiguration
    {
        public static void ConfigureCategory(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(x => x.Slug).IsUnique();

                entity.HasMany(p => p.Products)
                    .WithOne(c => c.Category)
                    .HasForeignKey(c => c.IdCategory)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(c => c.CategoryName)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Children)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(c => c.ParentId);
            });
        }
    }
}