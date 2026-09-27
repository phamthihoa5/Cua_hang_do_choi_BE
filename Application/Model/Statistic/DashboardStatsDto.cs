using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Statistics
{
    public class DashboardStatsDto
    {
        public InventoryStatsDto InventoryStats { get; set; }
        public UserCountStatsDto UserCountStats { get; set; }
    }

}
