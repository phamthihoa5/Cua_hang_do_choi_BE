using Application.Model.Product;
using Application.Model.Statistic;
using Application.Model.Statistics;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.IService.User
{
    public interface IStatisticService
    {
        Task<List<YearlyStatsDto>> GetYearlyStatsAsync(int year);
        Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync(int year, int month);
        Task<DailyOrderCountDto> GetOrderStatsByWarehouseAsync(
            DateTime day,
            TrangThaiDonHang orderStatus);

        Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime day);
        Task<DailyProfitDto> GetDailyProfitAsync(DateTime day);

        Task<List<ProductResponseDto>> GetTopSellingProductsAsync(
            int year,
            int month,
            int topN);

        Task<List<TopOrder>> GetOrdersAsync(int top = 10);
    }
}