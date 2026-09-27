using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string orderInfo, string bankCode = "");
        Task<bool> ValidateCallbackAsync(IDictionary<string, string> queryParams);
    }
}
