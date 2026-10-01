using Core.Common;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using static Core.Entities.Enum;

namespace Core.Entities;

[Table("Product")]
public class Product : BaseEntity, IAuditedEntity
{
    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public Guid IdSupplier { get; set; }

    public Guid IdCategory { get; set; }

    // Khuyến mãi
    public Guid? IdPromotion { get; set; }

    public string? Image { get; set; }

    public TrangThaiSanPham ProductStatus { get; set; }
        = TrangThaiSanPham.HetHang;

    public string Slug { get; set; } = null!;

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    // Navigation dành cho Promotion
    public Promotion? Promotion { get; set; }
}