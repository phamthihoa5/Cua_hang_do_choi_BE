using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Statistics
{
    public class MonthlyStatsDto
    {
        public int Day { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal ChiPhiNhap { get; set; }
        public decimal LoiNhuan => DoanhThu - ChiPhiNhap;
    }
}