using Application.Common.Models;
using Application.Model.API;
using Application.Model.Customer;
using Application.Model.New;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface ICustomerService
    {
        Task<PageList<CustomerResponseDto>> GetAllCustomersAsync(QueryParam query = null);
        Task<string> AccountLockAsync(Guid id);
        Task<string> RestoreAccountAsync(Guid id);
    }
}
