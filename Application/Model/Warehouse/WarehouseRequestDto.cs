using Application.Model.WarehouseDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.Warehouse
{
    public class WarehouseRequestDto
    {
        public List<WarehouseDetailRequestDto> Details { get; set; }
        public DateTimeOffset DateEntered { get; set; } = DateTime.UtcNow;
    }
}
