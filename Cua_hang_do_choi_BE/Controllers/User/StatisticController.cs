using Application.IService.User;
using Application.Model.API;
using Application.Model.Product;
using Application.Model.Statistic;
using Application.Model.Statistics;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using static Core.Entities.Enum;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[Controller]")]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        /// <summary>
        /// Lấy trạng thái sản phẩm trong kho và tổng người dùng theo ngày
        /// </summary>
        [HttpGet("warehouse")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime day)
        {
            var result = await _statisticService.GetDashboardStatsAsync(day);
            return Ok(result);
        }

        /// <summary>
        /// Lấy trạng thái đơn hàng theo ngày
        /// </summary>
        [HttpGet("order")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrderStats(
            [FromQuery] DateTime day,
            TrangThaiDonHang orderStatus)
        {
            var result = await _statisticService
                .GetOrderStatsByWarehouseAsync(day, orderStatus);

            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu, chi phí và lợi nhuận theo ngày
        /// </summary>
        [HttpGet("Profit")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDailyProfit([FromQuery] DateTime day)
        {
            var result = await _statisticService.GetDailyProfitAsync(day);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê doanh thu và chi phí theo năm.
        /// </summary>
        [HttpGet("yearly/{year}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<YearlyStatsDto>>> GetYearlyStatsAsync(int year)
        {
            var result = await _statisticService.GetYearlyStatsAsync(year);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thống kê doanh thu và chi phí theo tháng.
        /// </summary>
        [HttpGet("monthly/{year}/{month}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<MonthlyStatsDto>>> GetMonthlyStatsAsync(
            int year,
            int month)
        {
            var result = await _statisticService.GetMonthlyStatsAsync(year, month);
            return Ok(result);
        }

        /// <summary>
        /// Lấy các sản phẩm bán chạy nhất
        /// </summary>
        [HttpGet("product/{year}/{month}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProductDTO>>> GetTopProductsAsync(
            int year,
            int month,
            [FromQuery] int topN)
        {
            var topProducts = await _statisticService
                .GetTopSellingProductsAsync(year, month, topN);

            if (topProducts == null || !topProducts.Any())
                return NotFound(new
                {
                    message = "Không có sản phẩm bán trong tháng này."
                });

            return Ok(topProducts);
        }

        /// <summary>
        /// Lấy 10 đơn hàng gần nhất
        /// </summary>
        [HttpGet("toporder")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<TopOrder>>> GetTopOrderAsync(int top = 5)
        {
            var topOrder = await _statisticService.GetOrdersAsync(top);
            return Ok(topOrder);
        }
    }
}