using Application.Common.Models;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;

namespace API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Tạo mới sản phẩm.
        /// </summary>
        /// <remarks>
        /// Dùng để thêm sản phẩm mới.  
        /// Gửi dữ liệu qua <b>multipart/form-data</b> để hỗ trợ upload ảnh.
        /// </remarks>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] ProductRequestDto createProduct)
        {
            var result = await _productService.CreateProductAsync(createProduct);
            return Ok(ApiResult<ProductResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cần cập nhật</param>
        [HttpPut("{productId}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResult<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid productId, [FromForm] ProductRequestDto updateProduct)
        {
            var result = await _productService.UpdateProductAsync(productId, updateProduct);
            return Ok(ApiResult<ProductResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy danh sách sản phẩm (dành cho quản trị viên).
        /// </summary>
        /// <remarks>
        /// Hỗ trợ tìm kiếm, lọc, sắp xếp và phân trang.
        /// </remarks>
        [HttpGet("admin")]
        [ProducesResponseType(typeof(ApiResult<PageList<ProductResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] QueryParam? query = null)
        {
            var result = await _productService.GetAllAsync(query);
            return Ok(ApiResult<PageList<ProductResponseDto>>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy danh sách sản phẩm hiển thị cho khách hàng.
        /// </summary>
        /// <remarks>
        /// Cho phép lọc theo danh mục, khoảng giá, từ khóa, và sắp xếp theo giá hoặc tên sản phẩm.  
        /// Endpoint này dùng để hiển thị danh sách sản phẩm ở trang người dùng.
        /// </remarks>
        [HttpGet("client")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<PageList<ProductResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllClient([FromQuery] ProductFilterParam? query = null)
        {
            var result = await _productService.GetAllAsyncClient(query);
            return Ok(ApiResult<PageList<ProductResponseDto>>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm theo ID hoặc Slug.
        /// </summary>
        /// <param name="key">ID hoặc slug của sản phẩm</param>
        [HttpGet("{key}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdOrSlug(string key)
        {
            ProductResponseDto result;

            if (Guid.TryParse(key, out var productId))
                result = await _productService.GetByIdAsync(productId);
            else
                result = await _productService.GetBySlugAsync(key);

            return Ok(ApiResult<ProductResponseDto>.ReturnSuccess(result));
        }

        /// <summary>
        /// Xóa sản phẩm (xóa mềm).
        /// </summary>
        /// <param name="productId">ID sản phẩm cần xóa</param>
        [HttpDelete("{productId}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid productId)
        {
            var result = await _productService.DeleteProductAsync(productId);
            return Ok(ApiResult<string>.ReturnSuccess(result));
        }

        /// <summary>
        /// Lấy danh sách sản phẩm đã bị xóa mềm.
        /// </summary>
        /// <remarks>
        /// Cho phép quản trị viên xem và khôi phục các sản phẩm đã bị xóa.
        /// </remarks>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(ApiResult<PageList<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedProducts([FromQuery] QueryParam query)
        {
            var result = await _productService.GetIsDeletedAsync(query);
            return Ok(ApiResult<PageList<ProductResponseDto>>.ReturnSuccess(result));
        }

        /// <summary>
        /// Khôi phục sản phẩm đã xóa mềm.
        /// </summary>
        /// <param name="id">ID sản phẩm cần khôi phục</param>
        [HttpPut("restore/{id}")]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreProduct(Guid id)
        {
            var success = await _productService.RestoreProductAsync(id);
            return Ok(ApiResult<string>.ReturnSuccess(success));
        }

        /// <summary>
        /// Lấy 3 sản phẩm sắp hết khuyến mãi.
        /// </summary>
        /// <remarks>
        /// Dùng để hiển thị các sản phẩm có thời gian khuyến mãi gần kết thúc.
        /// </remarks>
        [HttpGet("product-end-promotion")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<List<ProductResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get3Product()
        {
            var result = await _productService.Get3ProductSaleAsync();
            return Ok(ApiResult<List<ProductResponseDto>>.ReturnSuccess(result));
        }
    }
}
