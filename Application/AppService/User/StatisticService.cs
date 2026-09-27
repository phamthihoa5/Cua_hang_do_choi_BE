using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.Product;
using Application.Model.Statistic;
using Application.Model.Statistics;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class StatisticService : IStatisticService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StatisticService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StatisticService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<StatisticService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DailyOrderCountDto> GetOrderStatsByWarehouseAsync(DateTime day, TrangThaiDonHang orderStatus)
        {
            var endOfDay = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc).AddDays(1);
            var orders = await _unitOfWork.orderRepo.GetAll()
                .Where(o => !o.IsDeleted && o.OrderDate < endOfDay)
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new DailyOrderCountDto
            {
                ChoXuLy = orders.FirstOrDefault(x => x.Status == TrangThaiDonHang.Chờ_xử_lý)?.Count ?? 0,
                XacNhan = orders.FirstOrDefault(x => x.Status == TrangThaiDonHang.Đã_xác_nhận)?.Count ?? 0,
                DangGiao = orders.FirstOrDefault(x => x.Status == TrangThaiDonHang.Đang_giao_hàng)?.Count ?? 0,
                DaGiao = orders.FirstOrDefault(x => x.Status == TrangThaiDonHang.Đã_giao_hàng)?.Count ?? 0,
                DaHuy = orders.FirstOrDefault(x => x.Status == TrangThaiDonHang.Đã_hủy)?.Count ?? 0
            };
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime day)
        {
            // Tổng số lượng còn hàng (Quantity > 0)
            var conHang = await _unitOfWork.productRepo.GetAll()
                .Where(p => !p.IsDeleted && p.Quantity > 0)
                .SumAsync(p => (int?)p.Quantity) ?? 0;
           
            var inventoryStats = new InventoryStatsDto
            {
                NewQuantity = conHang
            };
            var userStats = new UserCountStatsDto
            {
                TotalUsers = await _unitOfWork.userRepo.GetAll().CountAsync(u => !u.IsDeleted)
            };
            return new DashboardStatsDto
            {
                InventoryStats = inventoryStats,
                UserCountStats = userStats
            };
        }

        public async Task<DailyProfitDto> GetDailyProfitAsync(DateTime day)
        {
            var startOfDay = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var tongDoanhThu = await _unitOfWork.orderRepo.GetAll()
                .Where(o => !o.IsDeleted && o.OrderStatus != TrangThaiDonHang.Đã_hủy && o.OrderDate >= startOfDay && o.OrderDate < endOfDay)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var tongChiPhi = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(w => !w.IsDeleted && w.Warehouse.DateEntered >= startOfDay && w.Warehouse.DateEntered < endOfDay)
                .SumAsync(w => (decimal?)(w.ImportPrice * w.Quantity)) ?? 0;

            return new DailyProfitDto
            {
                Ngay = startOfDay,
                TongDoanhThu = tongDoanhThu,
                TongChiPhi = tongChiPhi,
                LoiNhuan = tongDoanhThu - tongChiPhi
            };
        }

        public async Task<List<MonthlyStatsDto>> GetMonthlyStatsAsync(int year, int month)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var orders = await _unitOfWork.orderRepo.GetAll()
                .Where(o => !o.IsDeleted && o.OrderDate.Year == year && o.OrderDate.Month == month && o.OrderStatus != TrangThaiDonHang.Đã_hủy)
                .ToListAsync();

            var doanhThu = orders
                .GroupBy(o => o.OrderDate.Day)
                .Select(g => new { Day = g.Key, Total = g.Sum(x => x.TotalPrice) })
                .ToList();

            var chiPhi = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(w => !w.IsDeleted && w.Warehouse.DateEntered.Year == year && w.Warehouse.DateEntered.Month == month)
                .GroupBy(w => w.Warehouse.DateEntered.Day)
                .Select(g => new { Day = g.Key, Total = g.Sum(x => x.ImportPrice * x.Quantity) })
                .ToListAsync();

            return Enumerable.Range(1, daysInMonth)
                .Select(d => new MonthlyStatsDto
                {
                    Day = d,
                    DoanhThu = doanhThu.FirstOrDefault(x => x.Day == d)?.Total ?? 0,
                    ChiPhiNhap = chiPhi.FirstOrDefault(x => x.Day == d)?.Total ?? 0
                })
                .ToList();
        }

        public async Task<List<YearlyStatsDto>> GetYearlyStatsAsync(int year)
        {
            var doanhThu = await _unitOfWork.orderRepo.GetAll()
                .Where(o => !o.IsDeleted && o.OrderDate.Year == year && o.OrderStatus != TrangThaiDonHang.Đã_hủy)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalPrice) })
                .ToListAsync();

            var chiPhi = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(w => !w.IsDeleted && w.Warehouse.DateEntered.Year == year)
                .GroupBy(w => w.Warehouse.DateEntered.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(w => w.ImportPrice * w.Quantity) })
                .ToListAsync();

            return Enumerable.Range(1, 12)
                .Select(m => new YearlyStatsDto
                {
                    Month = m,
                    DoanhThu = doanhThu.FirstOrDefault(x => x.Month == m)?.Total ?? 0,
                    ChiPhiNhap = chiPhi.FirstOrDefault(x => x.Month == m)?.Total ?? 0
                })
                .ToList();
        }

        public async Task<List<ProductResponseDto>> GetTopSellingProductsAsync(int year, int month, int topN)
        {
            // 1️⃣ Lấy danh sách Id sản phẩm bán chạy nhất trong tháng
            var topProductIds = await _unitOfWork.orderDetailsRepo.GetAll()
                .Include(od => od.Order)
                .Where(od => !od.Order.IsDeleted && od.Order.OrderStatus != TrangThaiDonHang.Đã_hủy && od.Order.OrderDate.Year == year && od.Order.OrderDate.Month == month)
                .GroupBy(od => od.IdProduct)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(topN)
                .Select(x => x.ProductId)
                .ToListAsync();

            if (!topProductIds.Any())
                return new List<ProductResponseDto>();

            // 2️⃣ Lấy thông tin sản phẩm chi tiết (có liên quan Supplier, Category, Promotion)
            var products = await _unitOfWork.productRepo.GetAll()
                .Include(p => p.Supplier)
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Promotion)
                .Where(p => topProductIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            // 3️⃣ Map sang DTO
            var mapped = _mapper.Map<List<ProductResponseDto>>(products);

            // 4️⃣ Sắp xếp lại theo thứ tự bán chạy (theo topProductIds)
            mapped = mapped.OrderBy(p => topProductIds.IndexOf(p.Id)).ToList();

            // 5️⃣ Trả về danh sách đã map
            return mapped;
        }

        public async Task<List<TopOrder>> GetOrdersAsync(int top = 10)
        {
            var latestOrders = await _unitOfWork.orderRepo.GetAll()
                .Include(o => o.User)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(top)
                .Select(o => new TopOrder
                {
                    CustomerName = o.User.FullName,
                    TotalPrice = o.TotalPrice,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return latestOrders;
        }
    }
}
