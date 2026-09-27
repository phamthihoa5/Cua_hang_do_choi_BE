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
[Table("OrderDetails")]
public class OrderDetails : BaseEntity, IAuditedEntity
{
    public Guid IdOrder { get; set; }
    public Guid IdProduct { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }

    //Navigation properties
    public Order Order { get; set; }
    public Product Product { get; set; }

    [DefaultValue(false)]
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }


}
