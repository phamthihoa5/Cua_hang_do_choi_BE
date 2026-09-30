using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigurations;

public class WarehouseDetailConfiguration : IEntityTypeConfiguration<WarehouseDetail>
{
    public void Configure(EntityTypeBuilder<WarehouseDetail> builder)
    {
        builder.ToTable("WarehouseDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.SupplierName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.ImportPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
