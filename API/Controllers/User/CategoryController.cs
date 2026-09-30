using Application.AppService.User;
using Application.Common.Models;
using Application.Exceptions;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Category;
using Application.Model.New;
using Application.Model.News;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Shared.Services.ClaimService;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CategoryController: ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        // Create
        /// <summary>
        /// Tạo danh mục
        /// </summary>
        [HttpPost("Category")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryRequest createCategory)
        {
            var result = await _categoryService.CreateCategoryAsync(createCategory);
            return Ok(ApiResult<CategoryResponse>.ReturnSuccess(result));
        }
        //getall
        /// <summary>
        /// lấy tất cả danh mục admin
        /// </summary>
        /// 
        [HttpGet("Admin")]
        //[Produces("application/json")]
        //[Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCategoryAsync([FromQuery] QueryParam? query = null)
        {
            var result = await _categoryService.GetAllAsync(query);
            return Ok(ApiResult<PageList<CategoryResponse>>.ReturnSuccess(result));
        }
        //getall
        /// <summary>
        /// lấy tất cả danh mục client
        /// </summary>
        /// 
        [HttpGet("Client")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCategoryClient([FromQuery] QueryParam? query = null)
        {
            var result = await _categoryService.GetAllAsyncClient(query);
            return Ok(ApiResult<PageList<CategoryResponse>>.ReturnSuccess(result));
        }
        //getall
        /// <summary>
        /// lấy tất cả danh mục đã xóa
        /// </summary>
        /// 
        [HttpGet("Delete")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCategoryDeltee([FromQuery] QueryParam? query = null)
        {
            var result = await _categoryService.GetAllAsyncDelete(query);
            return Ok(ApiResult<PageList<CategoryResponse>>.ReturnSuccess(result));
        }
        // get id
        /// <summary>
        /// Lấy theo id
        /// </summary>
        [HttpGet("{categoryId}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid categoryId)
        {
            var result = await _categoryService.GetByIdAsync(categoryId);
            return Ok(ApiResult<CategoryResponse>.ReturnSuccess(result));
        }
        //delete
        /// <summary>
        /// Xóa danh mục ( để force là true thì xóa cả danh mục con còn false thì k xóa)
        /// </summary>
        /// 
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategory(Guid categoryId, bool force = false)
        {
            var result = await _categoryService.DeleteCategoryAsync(categoryId, force);
            return Ok(ApiResult<string>.ReturnSuccess(result));
        }
        //update
        /// <summary>
        /// Sửa danh mục
        /// </summary>
        [HttpPut("{categoryId}")]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromForm] CategoryRequest updateCategory)
        {
            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateCategory);
            return Ok(ApiResult<CategoryResponse>.ReturnSuccess(result));
        }
        //restore
        /// <summary>
        /// Khôi phục danh mục
        /// </summary>
        [HttpPut("restore")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreCategory(Guid idcategory)
        {
            try
            {
                await _categoryService.RestoreCategoryAsync(idcategory);
                return Ok(ApiResult<string>.ReturnSuccess("Khôi phục thành công"));
            }
            catch (BadRequestException1 ex)
            {
                // Trả về JSON lỗi 400
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.Details
                });
            }
            catch (Exception ex)
            {
                // Trả về JSON lỗi hệ thống
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi hệ thống",
                    details = ex.Message
                });
            }
        }

    }
}
