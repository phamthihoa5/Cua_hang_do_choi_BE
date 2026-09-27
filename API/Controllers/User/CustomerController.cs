using Application.AppService.User;
using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Category;
using Application.Model.Customer;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api[Controller]")]
    public class CustomerController: ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        //getall
        /// <summary>
        /// lấy tất cả khách hàng
        /// </summary>
        /// 
        [HttpGet("Customer")]
        //[Produces("application/json")]
        //[Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCustomerAsync([FromQuery] QueryParam? query = null)
        {
            var result = await _customerService.GetAllCustomersAsync(query);
            return Ok(ApiResult<PageList<CustomerResponseDto>>.ReturnSuccess(result));
        }

        //restore
        /// <summary>
        /// Khóa tài khoản
        /// </summary>
        [HttpPut("lock")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AccountLock(Guid id)
        {
            await _customerService.AccountLockAsync(id);
            return Ok(ApiResult<string>.ReturnSuccess("Khóa tài khoản thành công"));

        }

        //restore
        /// <summary>
        /// Khôi phục tài khoản
        /// </summary>
        [HttpPut("restore")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreAccount(Guid id)
        {
            await _customerService.RestoreAccountAsync(id);
            return Ok(ApiResult<string>.ReturnSuccess("Khôi phục tài khoản thành công"));

        }
    }
}
