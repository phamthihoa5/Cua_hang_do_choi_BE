using API.Controllers.Common;
using Application.AppService.User;
using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Order;
using Application.Model.Promotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Tạo đơn hàng mới từ giỏ hàng
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] OrderRequestDto request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return Ok(ApiResult<OrderResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (admin hoặc user)
        /// </summary>
        [HttpPut("{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResult<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAsync(Guid orderId, [FromBody] UpdateOrderDto dto)
        {
            var result = await _orderService.UpdateOrderAsync(orderId, dto);
            return Ok(ApiResult<OrderResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Hủy đơn hàng (chỉ khi đang chờ xử lý)
        /// </summary>
        [HttpDelete("{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync(Guid orderId)
        {
            var result = await _orderService.DeleteOrderAsync(orderId);
            return Ok(ApiResult<string>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy tất cả đơn hàng của user đang đăng nhập
        /// </summary>
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResult<ListOrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var result = await _orderService.GetOrderByIdUserAsync();
            return Ok(ApiResult<ListOrderDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy tất cả đơn hàng (dành cho admin)
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResult<PageList<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryParam? query)
        {
            var result = await _orderService.GetAllOrdersAsync(query);
            return Ok(ApiResult<PageList<OrderResponseDto>>.ReturnSuccess(result));
        }
    }
}
