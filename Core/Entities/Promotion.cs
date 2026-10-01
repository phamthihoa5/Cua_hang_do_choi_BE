using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("Promotion")]
    public class Promotion : BaseEntity, IAuditedEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercent { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public Guid? ApprovedBy { get; set; }
        public bool IsApproved { get; set; }
        public ICollection<Product> Products { get; set; }
        public string Slug { get; set; }


        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
