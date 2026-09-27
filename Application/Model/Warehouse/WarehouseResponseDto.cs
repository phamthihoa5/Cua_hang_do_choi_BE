using Application.Model.Product;
using Application.Model.WarehouseDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.Warehouse
{
    public class WarehouseResponseDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateEntered { get; set; }
        public decimal TotalPrice { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public List<WarehouseDetailResponseDto> Details { get; set; }
    }

}
