using Application.IService.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        /// Lấy thông tin tổng quan tài chính.
        /// Có thể lọc theo khoảng thời gian.
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetFinanceSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                return BadRequest(new
                {
                    message = "Từ ngày không được lớn hơn đến ngày."
                });
            }

            var result = await _financeService
                .GetFinanceSummaryAsync(fromDate, toDate);

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách giao dịch tài chính.
        /// Có thể lọc theo khoảng thời gian.
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date > toDate.Value.Date)
            {
                return BadRequest(new
                {
                    message = "Từ ngày không được lớn hơn đến ngày."
                });
            }

            var result = await _financeService
                .GetTransactionsAsync(fromDate, toDate);

            return Ok(result);
        }
    }
}