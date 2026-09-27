using Core.Common;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Core.Entities;
[Table("Product")]
public class Product : BaseEntity, IAuditedEntity
{
    public string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid IdSupplier { get; set; }
    public Guid IdCategory { get; set; }
    public Guid? IdPromotion { get; set; }
    public string? Image { get; set; }
    public TrangThaiSanPham ProductStatus { get; set; } = TrangThaiSanPham.HetHang;
    public string Slug { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }

    //Navigation properties
    public Supplier Supplier { get; set; }
    public Category Category { get; set; }
    public Promotion? Promotion { get; set; }
    public ICollection<Cart> Carts { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
    public ICollection<WarehouseDetail> Warehouses { get; set; }

}

