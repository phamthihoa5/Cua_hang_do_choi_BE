
using Core.Common;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;
[Table("Supplier")]
public class Supplier : BaseEntity, IAuditedEntity
{
    public string SupplierName { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Note { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }

    //Navigation properties
    public virtual ICollection<Product> Products { get; set; }

}
