using Application.Common.Models;
using Application.Model.API;
using Application.Model.Category;
using Application.Model.New;
using Application.Model.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
   public interface ICategoryService
    {
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequest createCategory);
        Task<CategoryResponse> UpdateCategoryAsync(Guid id, CategoryRequest updateCategory);
        Task<string> DeleteCategoryAsync(Guid id, bool force = false);
        Task<PageList<CategoryResponse>> GetAllAsync(QueryParam query = null);
        Task<PageList<CategoryResponse>> GetAllAsyncDelete(QueryParam query = null);
        Task RestoreCategoryAsync(Guid id);
        Task<PageList<CategoryResponse>> GetAllAsyncClient(QueryParam query = null);
        Task<CategoryResponse> GetByIdAsync(Guid id);
    }
}
