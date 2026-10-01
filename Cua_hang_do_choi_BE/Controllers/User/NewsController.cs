using Application.AppService.User;
using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.New;
using Application.Model.News;
using Application.Model.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using System.Diagnostics.Metrics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[Controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newService;
        public NewsController(INewsService newService)
        {
            _newService = newService;
        }
        // Create
        /// <summary>
        /// Tạo tin tức
        /// </summary>
        [HttpPost("News")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromForm] NewsRequestDto createNews)
        {
            var result = await _newService.CreateNewsAsync(createNews);
            return Ok(ApiResult<NewsResponse>.ReturnSuccess(result));
        }
        //get all admin
        /// <summary>
        /// Lấy all cho admin 2 type: 1 đã duyệt, 2 chưa duyệt (or ngược lại)
        /// </summary>
        [HttpGet("Admin")]
        [ProducesResponseType(typeof(ApiResult<PageList<NewsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] QueryParam? query = null)
        {
            var result = await _newService.GetAllAsync(query);
            return Ok(ApiResult<PageList<NewsResponse>>.ReturnSuccess(result));
        }

        // get all client
        /// <summary>
        /// lấy ra cho client
        /// </summary>
        [HttpGet("Client")]
        // [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleName.Customer)]
        [ProducesResponseType(typeof(ApiResult<PageList<NewsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsyncClient([FromQuery] QueryParam? query = null)
        {
            var result = await _newService.GetAllAsyncClient(query);
            return Ok(ApiResult<PageList<NewsResponse>>.ReturnSuccess(result));
        }

        // get id
        /// <summary>
        /// Lấy theo id
        /// </summary>
        [HttpGet("{newsId}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid newsId)
        {
            var result = await _newService.GetByIdAsync(newsId);
            return Ok(ApiResult<NewsResponse>.ReturnSuccess(result));
        }

        //update
        /// <summary>
        /// Sửa tin tức
        /// </summary>
        [HttpPut("{newsId}")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<NewsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNews(Guid newsId, [FromForm] NewsRequestDto updateNews)
        {
            var result = await _newService.UpdateNewsAsync(newsId, updateNews);
            return Ok(ApiResult<NewsResponse>.ReturnSuccess(result));
        }

        //delete
        /// <summary>
        /// Xóa tin tức
        /// </summary>
        [HttpDelete("{newsId}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteNews(Guid newsId)
        {
            var result = await _newService.DeleteNewsAsync(newsId);
            return Ok(ApiResult<string>.ReturnSuccess(result));
        }

        //getall delete
        /// <summary>
        /// lấy tin tức đã xóa
        /// </summary>
        [HttpGet("Delete")]
        [ProducesResponseType(typeof(ApiResult<PageList<NewsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsyncDelete([FromQuery] QueryParam? query = null)
        {
            var result = await _newService.GetAllAsyncDelete(query);
            return Ok(ApiResult<PageList<NewsResponse>>.ReturnSuccess(result));
        }

        //restore
        /// <summary>
        /// Khôi phục tin tức
        /// </summary>
        [HttpPut("restore")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreNews(Guid idnews)
        {
            await _newService.RestoreNewsAsync(idnews);
            return Ok(ApiResult<string>.ReturnSuccess("Khôi phục thành công"));

        }

        // duyệt
        /// <summary>
        /// Duyệt tin tức
        /// </summary>
        [HttpPut("Approve")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveNewsAsync(Guid idnews)
        {
            await _newService.ApproveNewsAsync(idnews);
            return Ok(ApiResult<string>.ReturnSuccess("Duyệt thành công"));

        }

    }
}
