using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Statistics
{
    public class DailyProfitDto
    {
        public DateTime Ngay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongChiPhi { get; set; }
        public decimal LoiNhuan { get; set; }
    }
}