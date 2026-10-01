using API.Controllers.Common;
using Application.AppService.User;
using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Product;
using Application.Model.Promotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ApiController
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>
        /// Lấy danh sách khuyến mãi ( 1-đã duyệt, 2-chưa duyệt) (Admin/Manager)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<PageList<PromotionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryParam query)
        {
            var result = await _promotionService.GetAllAsync(query);
            return Ok(ApiResult<PageList<PromotionResponseDto>>.ReturnSuccess(result));
        }


        /// <summary>
        /// Lấy thông tin khuyến mãi theo Id hoặc Slug
        /// </summary>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(ApiResult<PromotionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(string key)
        {
            PromotionResponseDto? result;

            if (Guid.TryParse(key, out var id))
            {
                //tìm theo Id
                result = await _promotionService.GetByIdAsync(id);
            }
            else
            {
                //tìm theo Slug
                result = await _promotionService.GetBySlugAsync(key);
            }

            if (result == null)
                return NotFound(ApiResult<string>.ReturnFailure(new[] { "Không tìm thấy khuyến mãi" }));

            return Ok(ApiResult<PromotionResponseDto>.ReturnSuccess(result));
        }


        /// <summary>
        /// Tạo mới khuyến mãi (mặc định chờ duyệt)
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<PromotionResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromForm] PromotionRequestDto dto)
        {
            var result = await _promotionService.CreateAsync(dto);
            return Ok(ApiResult<PromotionResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Cập nhật khuyến mãi
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResult<PromotionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] PromotionRequestDto dto)
        {
            var result = await _promotionService.UpdateAsync(id, dto);
            return Ok(ApiResult<PromotionResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Xóa khuyến mãi (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _promotionService.DeleteAsync(id);
            return Ok(ApiResult<bool>.ReturnSuccess(result));
        }

        /// <summary>
        /// Duyệt khuyến mãi (Admin/Manager)
        /// </summary>
        [HttpPut("approve/{id:guid}")]
        [ProducesResponseType(typeof(ApiResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveAsync(Guid id)
        {
            var result = await _promotionService.ApproveAsync(id);
            return Ok(ApiResult<bool>.ReturnSuccess(result));
        }

        /// <summary>
        /// Danh sách KM bị xóa mềm
        /// </summary>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(ApiResult<PageList<PromotionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedPromotions([FromQuery] QueryParam query)
        {
            var result = await _promotionService.GetIsDeletedAsync(query);
            return Ok(ApiResult<PageList<PromotionResponseDto>>.ReturnSuccess(result));
        }

        /// <summary>
        /// Khôi phục khuyến mãi đã xóa mềm
        /// </summary>
        [HttpPut("restore/{id}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestorePromotion(Guid id)
        {
            var message = await _promotionService.RestorePromotionAsync(id);
            return Ok(ApiResult<string>.ReturnSuccess(message));
        }

    }
}
