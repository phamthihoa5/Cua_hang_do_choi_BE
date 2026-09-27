using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using static Core.Entities.Enum;

namespace Core.Entities
{
    [Table("Warehouse")]
    public class Warehouse : BaseEntity, IAuditedEntity
    {
        public DateTimeOffset DateEntered { get; set; } = DateTime.UtcNow;
        public TrangThaiKhoHang Status { get; set; }
        public decimal TotalPrice { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // ============================
        // NAVIGATION PROPERTIES
        // ============================
        public ICollection<WarehouseDetail> WarehouseDetails { get; set; } = new List<WarehouseDetail>();
    }
}
