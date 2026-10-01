using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.New;
using Application.Model.News;
using Application.Model.Product;
using Application.Model.Supplier;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class NewsService : INewsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<NewsService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NewsService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ICloudinaryService cloudinaryService,
            ILogger<NewsService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        // duyệt
        public async Task ApproveNewsAsync(Guid newsIds, QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var newsList = await _unitOfWork.newRepo.GetAll()
                .Where(n => n.Id == newsIds && !n.IsDeleted)
                .ToListAsync();

            if (newsList == null || !newsList.Any())
                throw new BadRequestException("Không tìm thấy tin tức nào");

            foreach (var news in newsList)
            {
                if (!news.IsApproved) // chỉ update khi chưa duyệt
                {
                    news.IsApproved = true;
                    news.UpdatedBy = userId.ToString();
                    news.UpdatedOn = DateTime.UtcNow;
                }
            }
            var items = _mapper.Map<List<NewsResponse>>(newsList);
            foreach (var n in items)
            {
                if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                {
                    n.CreatedbyStr = _unitOfWork.userRepo.GetById(createdByGuid)
                                     .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu không phải GUID thì gán thẳng chuỗi CreatedBy (vd: "admin")
                    n.CreatedbyStr = !string.IsNullOrEmpty(n.CreatedBy) ? n.CreatedBy : "system";
                }
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task<NewsResponse> CreateNewsAsync(NewsRequestDto createNews)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var exists = await _unitOfWork.newRepo
                .GetAll()
                .Where(p => p.Title == createNews.Title && !p.IsDeleted)
                .FirstOrDefaultAsync();

            var news = new News
            {
                Title = createNews.Title,
                Content = createNews.Content,
                IsApproved = false,
                CreatedBy = userId.ToString(),
                CreatedOn = DateTime.UtcNow
            };

            if (createNews.Image != null && createNews.Image.Any())
            {
                var imagePaths = new List<string>();
                foreach (var file in createNews.Image)
                {
                    var path = await _cloudinaryService.UploadImageAsync(file);
                    imagePaths.Add(path);
                }
                news.Image = JsonConvert.SerializeObject(imagePaths);
            }
            await _unitOfWork.newRepo.Add(news);
            await _unitOfWork.CompleteAsync();
            var response = _mapper.Map<NewsResponse>(news);
            if (!string.IsNullOrEmpty(response.CreatedBy))
            {
                response.CreatedbyStr = _unitOfWork.userRepo
                    .GetById(Guid.NewGuid())
                    .FirstOrDefault()?.FullName ?? "system";
            }

            return response;
        }

        public async Task<string> DeleteNewsAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var news = await _unitOfWork.newRepo.GetById(id).FirstOrDefaultAsync();
            if (news == null)
                throw new BadRequestException("News not found");

            news.IsDeleted = true;
            news.UpdatedBy = userId.ToString();
            news.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.newRepo.Update(news);
            await _unitOfWork.CompleteAsync();


            return "Đã xóa tin tức";
        }

        public async Task<PageList<NewsResponse>> GetAllAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var news = _unitOfWork.newRepo.GetAll()
                        .Where(n => !n.IsDeleted);
            // Lọc theo search nếu có
            if (!string.IsNullOrEmpty(query?.Search))
            {
                news = news.Where(n => n.Title.ToLower().Contains(query.Search));
            }
            if (query?.Type != null)
            {
                switch (query.Type)
                {
                    case 1:
                        news = news.Where(n => n.IsApproved == true);
                        break;
                    case 2:
                        news = news.Where(n => n.IsApproved == false);
                        break;
                }
            }

            var totalCount = await news.CountAsync();

            // Phân trang
            var newsList = await news
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                .Take(query?.PageSize ?? 20)
                .ToListAsync();

            var items = _mapper.Map<List<NewsResponse>>(newsList);
            foreach (var n in items)
            {
                if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                {
                    n.CreatedbyStr = _unitOfWork.userRepo.GetById(createdByGuid)
                                     .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu không phải GUID thì gán thẳng chuỗi CreatedBy (vd: "admin")
                    n.CreatedbyStr = !string.IsNullOrEmpty(n.CreatedBy) ? n.CreatedBy : "system";
                }
            }
            return new PageList<NewsResponse>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 20);

        }
        public async Task<PageList<NewsResponse>> GetAllAsyncClient(QueryParam query = null)
        {
            var news = _unitOfWork.newRepo.GetAll()
                        .Where(n => !n.IsDeleted && n.IsApproved);
            // Lọc theo search nếu có
            if (!string.IsNullOrEmpty(query?.Search))
            {
                news = news.Where(n => n.Title.ToLower().Contains(query.Search));
            }
            news = news.OrderByDescending(n => n.UpdatedOn ?? DateTime.MinValue);
            var totalCount = await news.CountAsync();

            // Phân trang
            var newsList = await news
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                .Take(query?.PageSize ?? 20)
                .ToListAsync();

            var items = _mapper.Map<List<NewsResponse>>(newsList);
            foreach (var n in items)
            {
                if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                {
                    n.CreatedbyStr = _unitOfWork.userRepo.GetById(createdByGuid)
                                     .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu không phải GUID thì gán thẳng chuỗi CreatedBy (vd: "admin")
                    n.CreatedbyStr = !string.IsNullOrEmpty(n.CreatedBy) ? n.CreatedBy : "system";
                }
            }
            return new PageList<NewsResponse>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 20);
        }

        public async Task<PageList<NewsResponse>> GetAllAsyncDelete(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var news = _unitOfWork.newRepo.GetAll()
                        .Where(n => n.IsDeleted);
            // Lọc theo search nếu có
            if (!string.IsNullOrEmpty(query?.Search))
            {
                news = news.Where(n => n.Title.ToLower().Contains(query.Search));
            }


            var totalCount = await news.CountAsync();

            // Phân trang
            var newsList = await news
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                .Take(query?.PageSize ?? 20)
                .ToListAsync();

            var items = _mapper.Map<List<NewsResponse>>(newsList);
            foreach (var n in items)
            {
                if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                {
                    n.CreatedbyStr = _unitOfWork.userRepo.GetById(createdByGuid)
                                     .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu không phải GUID thì gán thẳng chuỗi CreatedBy (vd: "admin")
                    n.CreatedbyStr = !string.IsNullOrEmpty(n.CreatedBy) ? n.CreatedBy : "system";
                }
            }
            return new PageList<NewsResponse>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 20);
        }

        public async Task<NewsResponse> GetByIdAsync(Guid id)
        {
            var news = await _unitOfWork.newRepo.GetAll()
               .FirstOrDefaultAsync((n => n.Id == id));

            if (news == null)
                throw new BadRequestException("News not found");
            return _mapper.Map<NewsResponse>(news);
        }

        public async Task RestoreNewsAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            // Lấy danh sách bản tin đang bị xóa theo danh sách id truyền vào
            var newsList = await _unitOfWork.newRepo
                .GetAll()
                .Where(n => n.Id == id && n.IsDeleted)
                .ToListAsync();

            if (newsList == null || !newsList.Any())
                return;

            foreach (var news in newsList)
            {
                news.IsDeleted = false;              // Khôi phục
                news.UpdatedBy = userId.ToString();            // Ghi nhận người khôi phục
                news.ApprovedBy = Guid.Parse(await _tokenService.GetUserIdFromTokenAsync());
                news.UpdatedOn = DateTime.UtcNow;   // Ghi nhận thời gian
            }
            var items = _mapper.Map<List<NewsResponse>>(newsList);
            foreach (var n in items)
            {
                if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                {
                    n.CreatedbyStr = _unitOfWork.userRepo.GetById(createdByGuid)
                                     .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu không phải GUID thì gán thẳng chuỗi CreatedBy (vd: "admin")
                    n.CreatedbyStr = !string.IsNullOrEmpty(n.CreatedBy) ? n.CreatedBy : "system";
                }
            }
            await _unitOfWork.CompleteAsync();

        }

        public async Task<NewsResponse> UpdateNewsAsync(Guid newsId, NewsRequestDto updateNews)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var news = await _unitOfWork.newRepo.GetAll()
                .Where(n => n.Id == newsId && !n.IsDeleted)
                .FirstOrDefaultAsync();

            if (news == null)
                throw new BadRequestException("News not found");

            // Cập nhật nếu có giá trị mới, còn null thì giữ nguyên
            news.Title = string.IsNullOrWhiteSpace(updateNews.Title) ? news.Title : updateNews.Title;
            news.Content = string.IsNullOrWhiteSpace(updateNews.Content) ? news.Content : updateNews.Content;

            // Cập nhật ảnh nếu có
            if (updateNews.Image != null && updateNews.Image.Any())
            {
                var imageUrls = new List<string>();
                foreach (var file in updateNews.Image)
                {
                    var url = await _cloudinaryService.UploadImageAsync(file);
                    imageUrls.Add(url);
                }

                news.Image = JsonConvert.SerializeObject(imageUrls);
            }

            // Luôn update thời gian và user
            news.CreatedBy = userId.ToString();
            news.UpdatedBy = userId.ToString();
            news.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            var response = _mapper.Map<NewsResponse>(news);
            if (!string.IsNullOrEmpty(response.CreatedBy))
            {
                if (Guid.TryParse(response.CreatedBy, out var createdByGuid))
                {
                    // Nếu là GUID hợp lệ -> tìm tên trong userRepo
                    response.CreatedbyStr = _unitOfWork.userRepo
                        .GetById(createdByGuid)
                        .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    // Nếu chỉ là chuỗi (vd: "admin") -> giữ nguyên
                    response.CreatedbyStr = response.CreatedBy;
                }
            }
            else
            {
                response.CreatedbyStr = "system";
            }

            return response;
        }

    }
}
