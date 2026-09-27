using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.Statistic
{
    public class TopOrder
    {
        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public TrangThaiDonHang OrderStatus { get; set; }

        public DateTime OrderDate { get; set; }
    }
}