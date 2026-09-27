using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Statistics
{
    public class YearlyStatsDto
    {
        public int Month { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal ChiPhiNhap { get; set; }
        public decimal LoiNhuan => DoanhThu - ChiPhiNhap;
    }
}