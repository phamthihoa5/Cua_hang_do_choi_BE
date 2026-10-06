using Application.Common.Models;
using Application.Model.API;
using Application.Model.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductRequestDto createProduct);
        Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductRequestDto updateProduct);
        Task<string> DeleteProductAsync(Guid id);
        Task<PageList<ProductResponseDto>> GetAllAsync(QueryParam query = null);
        Task<PageList<ProductResponseDto>> GetAllAsyncClient(ProductFilterParam filter);
        Task<ProductResponseDto> GetByIdAsync(Guid id);
        Task<ProductResponseDto> GetBySlugAsync(string slug);
        Task<PageList<ProductResponseDto>> GetIsDeletedAsync(QueryParam query = null);
        Task<string> RestoreProductAsync(Guid id);
        Task<List<ProductResponseDto>> Get3ProductSaleAsync();
    }
}