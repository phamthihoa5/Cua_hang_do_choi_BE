using Application.Common.Models;
using Application.Model.API;
using Application.Model.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderRequestDto createOrder);
        Task<ListOrderDto> GetOrderByIdUserAsync();
        Task<PageList<OrderResponseDto>> GetAllOrdersAsync(QueryParam? query = null);
        Task<OrderResponseDto> UpdateOrderAsync(Guid userId, UpdateOrderDto updateOrder);
        Task<string> DeleteOrderAsync(Guid userId);
    }
}
