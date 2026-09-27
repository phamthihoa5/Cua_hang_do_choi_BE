using API.Hubs;
using Application.IService;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public class OrderNotifier : IOrderNotifier
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotifier(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyOrderStatusChangedAsync(Guid orderId, string? ghnOrderCode, int orderStatus, string statusName, string? ghnStatus)
        {
            Console.WriteLine($"📢 SignalR Broadcast: Order {orderId} status changed to {orderStatus} ({statusName})");
            await _hubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", new
            {
                orderId = orderId,
                ghnOrderCode = ghnOrderCode,
                orderStatus = orderStatus,
                statusName = statusName,
                ghnStatus = ghnStatus
            });
        }
    }
}
