using Application.Model.Order;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IPaymentService
    {
        Task<object> CreatePaymentAsync(CreateOrderDto dto);
        Task<string> HandleReturnUrlAsync(IDictionary<string, string> queryParams);
        Task<object> HandleIpnAsync(IDictionary<string, string> queryParams);
    }
}
