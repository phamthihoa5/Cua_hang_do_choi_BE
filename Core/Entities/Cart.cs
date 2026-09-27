using Core.Common;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;
[Table("Cart")]
public class Cart : BaseEntity, IAuditedEntity
{
    public Guid IdProduct { get; set; }
    public Guid IdUser { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    //Navigation properties
    public virtual Product Product { get; set; }
    public ApplicationUser User { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
