using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.WarehouseDetail
{
    public class WarehouseDetailResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public Guid WarehouseId { get; set; }
        //public string WarehouseName { get; set; }

        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal TotalPrice => Quantity * ImportPrice;
        public TrangThaiKhoHang Status { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
