using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.WarehouseDetail
{
    public class WarehouseDetailRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
    }
}
