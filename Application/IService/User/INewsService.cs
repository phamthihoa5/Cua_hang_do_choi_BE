using Application.Common.Models;
using Application.Model.API;
using Application.Model.New;
using Application.Model.News;
using Application.Model.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface INewsService
    {
        Task<NewsResponse> CreateNewsAsync(NewsRequestDto createNews);
        Task<NewsResponse> UpdateNewsAsync(Guid id, NewsRequestDto updateNews);
        Task<string> DeleteNewsAsync(Guid id);
        Task<PageList<NewsResponse>> GetAllAsync(QueryParam query = null);
        Task<PageList<NewsResponse>> GetAllAsyncDelete(QueryParam query = null);
        Task RestoreNewsAsync(Guid id);
        Task<PageList<NewsResponse>> GetAllAsyncClient(QueryParam query = null);
        Task<NewsResponse> GetByIdAsync(Guid id);
        Task ApproveNewsAsync(Guid id, QueryParam query = null);
    }
}
