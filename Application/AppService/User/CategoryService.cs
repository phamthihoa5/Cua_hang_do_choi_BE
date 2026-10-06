using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Category;
using Application.Model.New;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Services.ClaimService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CategoryService(IMapper mapper, IUnitOfWork unitOfWork, ITokenService tokenService, ICloudinaryService cloudinaryService, ILogger<ProductService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest createCategory)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            // Kiểm tra trùng tên
            var exists = await _unitOfWork.categoryRepo.GetAll()
                            .Where(c => c.CategoryName == createCategory.CategoryName && !c.IsDeleted)
                            .FirstOrDefaultAsync();

            if (exists != null)
                throw new BadRequestException("Danh mục đã tồn tại.");
            var category = new Category
            {
                CategoryName = createCategory.CategoryName,
                ParentId = createCategory.ParentId,
                CreatedBy = userId.ToString(),
                CreatedOn = DateTime.UtcNow              
            };
            if (createCategory.Image != null && createCategory.Image.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(createCategory.Image);
                category.Image = imageUrl;
            }
            await _unitOfWork.categoryRepo.Add(category);
            await _unitOfWork.CompleteAsync();         
            return _mapper.Map<CategoryResponse>(category);
        }
        public async Task<CategoryResponse> UpdateCategoryAsync(Guid id, CategoryRequest updateCategory)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var oldCategory = _unitOfWork.categoryRepo.GetById(id).FirstOrDefault()
                                ?? throw new BadRequestException("Không tìm thấy danh mục này");
           
            var category = await _unitOfWork.categoryRepo.GetAll()
                .Where(c => c.CategoryName == updateCategory.CategoryName
                         && c.Id != id     // loại trừ chính danh mục đang cập nhật
                         && !c.IsDeleted)
                .FirstOrDefaultAsync();
            if (category != null)
                throw new BadRequestException("Danh mục này đã tồn tại");
            oldCategory.CategoryName = string.IsNullOrWhiteSpace(updateCategory.CategoryName)
                                    ? oldCategory.CategoryName  // giữ nguyên tên cũ nếu không nhập
                                    : updateCategory.CategoryName;
            // Cập nhật ảnh nếu có
            if (updateCategory.Image != null && updateCategory.Image.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(updateCategory.Image);
                oldCategory.Image = imageUrl;
            }
            oldCategory.ParentId = updateCategory.ParentId ?? oldCategory.ParentId;
            oldCategory.UpdatedBy = userId.ToString();
            oldCategory.UpdatedOn = DateTime.UtcNow;
           
            await _unitOfWork.categoryRepo.Update(oldCategory);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<CategoryResponse>(oldCategory);
        }
        public async Task RestoreCategoryAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var oldCategory = _unitOfWork.categoryRepo.GetById(id).FirstOrDefault()
                              ?? throw new BadRequestException("Không tìm thấy danh mục này");
            if (oldCategory.ParentId.HasValue)
            {
                var oldCategoryParent = _unitOfWork.categoryRepo.GetById(oldCategory.ParentId.Value).FirstOrDefault();
                if (oldCategoryParent!.IsDeleted)
                    throw new BadRequestException1($"Bạn hãy phục hồi danh mục cha {oldCategoryParent.CategoryName}");
            }

            oldCategory.IsDeleted = false;
            oldCategory.UpdatedBy = userId.ToString();
            oldCategory.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.categoryRepo.Update(oldCategory);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<string> DeleteCategoryAsync(Guid id, bool force = false)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            // Lấy danh mục cần xóa
            var category = await _unitOfWork.categoryRepo.GetAll()
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (category == null)
                throw new BadRequestException("Không tìm thấy danh mục");

            // Kiểm tra xem có danh mục con không
            var subCategories = await _unitOfWork.categoryRepo.GetAll()
                .Where(c => c.ParentId == id && !c.IsDeleted)
                .ToListAsync();

            // Nếu có danh mục con và chưa chọn xóa toàn bộ
            if (subCategories.Any() && !force)
            {
                throw new BadRequestException("Danh mục này có chứa danh mục con. Nếu bạn muốn xóa tất cả, hãy xác nhận lại (force = true) nếu muốn xóa danh mục cha.");
            }

            // 🧾 BẮT ĐẦU TRANSACTION ĐẢM BẢO XÓA DANH MỤC CHA VÀ CON TOÀN VẸN
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Nếu force = true thì xóa luôn cả danh mục con
                if (subCategories.Any() && force)
                {
                    foreach (var sub in subCategories)
                    {
                        sub.IsDeleted = true;
                        sub.UpdatedBy = userId.ToString();
                        sub.UpdatedOn = DateTime.UtcNow;
                        await _unitOfWork.categoryRepo.Update(sub);
                    }
                }
                // Xóa danh mục hiện tại
                category.IsDeleted = true;
                category.UpdatedBy = userId.ToString();
                category.UpdatedOn = DateTime.UtcNow;
                await _unitOfWork.categoryRepo.Update(category);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitAsync();

                return subCategories.Any()
                    ? "Đã xóa danh mục và tất cả danh mục con."
                    : "Đã xóa danh mục.";
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<PageList<CategoryResponse>> GetAllAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            // Lấy toàn bộ danh mục (không bị xóa)
            var categories = await _unitOfWork.categoryRepo.GetAll()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CreatedOn)
                .ToListAsync();
            //  Map sang DTO
            var categoryDtos = _mapper.Map<List<CategoryResponse>>(categories);
            // Gắn parentName 
            foreach (var cat in categoryDtos)
            {
                if (cat.ParentId != null)
                {
                    var parent = categoryDtos.FirstOrDefault(c => c.Id == cat.ParentId);
                    cat.ParentName = parent?.CategoryName;
                }
            }
            // Xây dựng cây danh mục (chỉ để các danh mục cha ở cấp gốc)
            foreach (var parent in categoryDtos)
            {
                parent.Children = categoryDtos
                    .Where(c => c.ParentId == parent.Id)
                    .ToList();
            }
            // Chỉ giữ danh mục cha ở cấp gốc
            var rootCategories = categoryDtos
                .Where(c => c.ParentId == null)
                .ToList();
            // Áp dụng tìm kiếm & phân trang
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var searchLower = query.Search.ToLower();
                rootCategories = rootCategories
                    .Where(c => c.CategoryName.ToLower().Contains(searchLower))
                    .ToList();
            }
            var totalCount = rootCategories.Count;
            var pageNumber = query?.PageNumber ?? 1;
            var pageSize = query?.PageSize ?? 20;

            var pagedItems = rootCategories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PageList<CategoryResponse>(pagedItems, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 1000);
        }
        public async Task<PageList<CategoryResponse>> GetAllAsyncClient(QueryParam query = null)
        {
            var category = _unitOfWork.categoryRepo.GetAll()
                           .Include(c => c.Parent)
                           .Where(c => !c.IsDeleted);
            if (!string.IsNullOrEmpty(query?.Search))
            {
                category = category.Where(c => c.CategoryName.ToLower().Contains(query.Search));
            }
            var totalCount = await category.CountAsync();
            var categoryList = await category.Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                                             .Take(query?.PageSize ?? 20).ToListAsync();

            var items = _mapper.Map<List<CategoryResponse>>(categoryList);
            foreach (var cate in items)
            {
                if (cate.ParentId.HasValue)
                {
                    cate.ParentName = _unitOfWork.categoryRepo.GetById(cate.ParentId.Value).FirstOrDefault()?.CategoryName;
                }
            }
            return new PageList<CategoryResponse>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 20);
        }

        public async Task<PageList<CategoryResponse>> GetAllAsyncDelete(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            // Lấy toàn bộ danh mục (cả bị xóa lẫn chưa)
            var allCategories = await _unitOfWork.categoryRepo.GetAll().ToListAsync();

            // Lọc riêng danh mục bị xóa để hiển thị
            var deletedCategories = allCategories.Where(c => c.IsDeleted).ToList();
            var categoryDtos = _mapper.Map<List<CategoryResponse>>(deletedCategories);

            //  Gán tên cha (ParentName)
            foreach (var cat in categoryDtos)
            {
                if (cat.ParentId != null)
                {
                    var parent = allCategories.FirstOrDefault(p => p.Id == cat.ParentId);
                    cat.ParentName = parent?.CategoryName;
                }
            }
            //Xây cây danh mục
            foreach (var parent in categoryDtos)
            {
                parent.Children = categoryDtos
                    .Where(c => c.ParentId == parent.Id)
                    .ToList();
            }
            //Chỉ lấy danh mục cha (ParentId == null)
            var rootCategories = categoryDtos
                                .Where(c => c.ParentId == null || !allCategories.Any(p => p.Id == c.ParentId && p.IsDeleted))
                                .ToList();

            //  Áp dụng tìm kiếm
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var searchLower = query.Search.ToLower();
                rootCategories = rootCategories
                    .Where(c => c.CategoryName.ToLower().Contains(searchLower))
                    .ToList();
            }
            //  Phân trang
            var totalCount = rootCategories.Count;
            var pageNumber = query?.PageNumber ?? 1;
            var pageSize = query?.PageSize ?? 20;

            var pagedItems = rootCategories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return new PageList<CategoryResponse>(pagedItems, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 20);
        }

        public async Task<CategoryResponse> GetByIdAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var category = await _unitOfWork.categoryRepo.GetById(id)
                           .Include(c => c.Parent)
                           .FirstOrDefaultAsync();

            if (category == null)
                throw new BadRequestException("News not found");
            return _mapper.Map<CategoryResponse>(category);
        }
    }
}
