using System;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IOrderNotifier
    {
        Task NotifyOrderStatusChangedAsync(Guid orderId, string? ghnOrderCode, int orderStatus, string statusName, string? ghnStatus);
    }
}
