using Application.AppService.User;
using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.User;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[Controller]")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        /// <summary> Danh sách nhân viên (phân trang)</summary>
        [HttpGet("staff")]
        [ProducesResponseType(typeof(PageList<UserProfileDto>), 200)]
        public async Task<IActionResult> GetAllStaff([FromQuery] QueryParam query)
        {
            var result = await _staffService.GetAllStaffAsync(query);
            return Ok(result);
        }

        /// <summary>Lấy chi tiết nhân viên theo ID</summary>
        [HttpGet("staff/{staffId}")]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStaffById(Guid staffId)
        {
            var result = await _staffService.GetStaffByIdAsync(staffId);
            return Ok(result);
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
            await _staffService.LockStaffAsync(id);
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
            await _staffService.RestoreStaffAsync(id);
            return Ok(ApiResult<string>.ReturnSuccess("Khôi phục tài khoản thành công"));

        }
    }
}
