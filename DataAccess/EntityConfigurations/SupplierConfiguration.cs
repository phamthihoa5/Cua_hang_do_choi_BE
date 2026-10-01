
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
	public void Configure(EntityTypeBuilder<Supplier> builder)
	{
		builder.ToTable("Suppliers");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.ValueGeneratedOnAdd();

		builder.Property(x => x.Name)
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(x => x.Phone)
			.HasMaxLength(20)
			.IsRequired();

		builder.Property(x => x.Email)
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(x => x.Address)
			.HasMaxLength(500)
			.IsRequired();

		builder.Property(x => x.Note)
			.HasMaxLength(1000);

		builder.Property(x => x.CreatedBy)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(x => x.UpdatedBy)
			.HasMaxLength(100);

		builder.Property(x => x.DeletedBy)
			.HasMaxLength(100);

		builder.Property(x => x.CreatedAt)
			.IsRequired();

		builder.Property(x => x.IsDeleted)
			.IsRequired();
	}
}
