using Core.Common;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("warehousedetail")]
    public class WarehouseDetail : BaseEntity, IAuditedEntity
    {
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }

        public int Quantity { get; set; }

        [NotMapped] // Không lưu vào DB, vì đây là cột tính toán
        public decimal TotalPrice => Quantity * ImportPrice;

        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        // ============================
        // AUDIT FIELDS
        // ============================
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // ============================
        // NAVIGATION
        // ============================
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
