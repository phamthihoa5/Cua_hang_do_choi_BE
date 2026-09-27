using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Services.ClaimService;

namespace Application.AppService.User
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<PromotionResponseDto> CreateAsync(PromotionRequestDto dto)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            if (dto.ProductIds == null || !dto.ProductIds.Any())
                throw new BadRequestException("Phải chọn ít nhất một sản phẩm cho khuyến mãi.");

            // Lấy sản phẩm từ DB
            var products = await _unitOfWork.productRepo
                .GetAll()
                .Where(p => dto.ProductIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            if (!products.Any())
                throw new BadRequestException("Không tìm thấy sản phẩm hợp lệ.");

            // Map DTO -> Entity
            var promotion = _mapper.Map<Promotion>(dto);
            promotion.Id = Guid.NewGuid();
            promotion.IsApproved = false;
            promotion.ApprovedBy = null;

            // Gắn danh sách sản phẩm vào promotion
            promotion.Products = products;

            // Lưu xuống DB
            await _unitOfWork.promotionRepo.Add(promotion);
            await _unitOfWork.CompleteAsync();

            // Map lại sang DTO để trả ra FE
            var result = _mapper.Map<PromotionResponseDto>(promotion);
            return result;
        }


        public async Task<PromotionResponseDto> UpdateAsync(Guid id, PromotionRequestDto dto)
        {
            var promotion = await _unitOfWork.promotionRepo.GetById(id)
                .Include(p => p.Products)
                .FirstOrDefaultAsync();

            if (promotion == null || promotion.IsDeleted)
                throw new NotFoundException("Không tìm thấy khuyến mãi");

            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            promotion.Title = dto.Title;
            promotion.Description = dto.Description;
            promotion.DiscountPercent = dto.DiscountPercent ?? promotion.DiscountPercent;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;

            if (dto.ProductIds != null && dto.ProductIds.Any())
            {
                var products = await _unitOfWork.productRepo
                    .GetAll()
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (!products.Any())
                    throw new BadRequestException("Không tìm thấy sản phẩm hợp lệ");

                promotion.Products = products;
            }

            await _unitOfWork.promotionRepo.Update(promotion);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PromotionResponseDto>(promotion);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var promotion = await _unitOfWork.promotionRepo.GetById(id).FirstOrDefaultAsync();
            if (promotion == null || promotion.IsDeleted)
                throw new NotFoundException("Không tìm thấy khuyến mãi");

            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            promotion.IsDeleted = true;

            await _unitOfWork.promotionRepo.Update(promotion);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<PromotionResponseDto?> GetByIdAsync(Guid id)
        {
            var promotion = await _unitOfWork.promotionRepo
                .GetById(id)
                .Include(p => p.Products)
                .FirstOrDefaultAsync();

            if (promotion == null || promotion.IsDeleted)
                return null;

            return _mapper.Map<PromotionResponseDto>(promotion);
        }

        public async Task<PromotionResponseDto?> GetBySlugAsync(string slug)
        {
            var promotion = await _unitOfWork.promotionRepo
                .GetAll()
                .Include(p => p.Products)
                .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted);

            return promotion == null ? null : _mapper.Map<PromotionResponseDto>(promotion);
        }

        public async Task<PageList<PromotionResponseDto>> GetAllAsync(QueryParam query = null)
        {
            // Truy vấn cơ sở dữ liệu cho tất cả các khuyến mãi chưa bị xóa
            var source = _unitOfWork.promotionRepo
                .GetAll()
                .Include(p => p.Products)
                .Where(p => !p.IsDeleted);

            // Tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var search = query.Search.ToLower();
                source = source.Where(p =>
                    p.Title.ToLower().Contains(search) ||
                    (p.Description != null && p.Description.ToLower().Contains(search))
                );
            }

            // Xử lý phân loại khuyến mãi dựa trên `Type` (đã duyệt, chưa duyệt)
            if (query?.Type.HasValue == true)
            {
                switch (query.Type)
                {
                    case 1:
                        // Nếu Type = 1, chỉ lấy khuyến mãi đã duyệt
                        source = source.Where(p => p.IsApproved == true);
                        break;

                    case 2:
                        // Nếu Type = 2, chỉ lấy khuyến mãi chưa duyệt
                        source = source.Where(p => p.IsApproved == false);
                        break;

                    // Thêm các case khác nếu cần
                    default:
                        break;
                }
            }

            // Áp dụng sắp xếp (nếu có)
            if (query?.Sorts != null && query.Sorts.Any())
            {
                IOrderedQueryable<Promotion> ordered = null;
                foreach (var sort in query.Sorts)
                {
                    if (sort.Key.Equals("title", StringComparison.OrdinalIgnoreCase))
                    {
                        ordered = sort.Sort == 1
                            ? (ordered == null ? source.OrderBy(p => p.Title) : ordered.ThenBy(p => p.Title))
                            : (ordered == null ? source.OrderByDescending(p => p.Title) : ordered.ThenByDescending(p => p.Title));
                    }
                    else if (sort.Key.Equals("createdOn", StringComparison.OrdinalIgnoreCase))
                    {
                        ordered = sort.Sort == 1
                            ? (ordered == null ? source.OrderBy(p => p.CreatedOn) : ordered.ThenBy(p => p.CreatedOn))
                            : (ordered == null ? source.OrderByDescending(p => p.CreatedOn) : ordered.ThenByDescending(p => p.CreatedOn));
                    }
                }

                if (ordered != null)
                    source = ordered;
            }
            else
            {
                // Sắp xếp mặc định theo ngày tạo giảm dần
                source = source.OrderByDescending(p => p.CreatedOn);
            }

            // Phân trang
            var pageNumber = query?.PageNumber ?? 1;
            var pageSize = query?.PageSize ?? 10;

            var totalCount = await source.CountAsync();
            var promotions = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = _mapper.Map<IEnumerable<PromotionResponseDto>>(promotions);

            return new PageList<PromotionResponseDto>(
                data,
                totalCount,
                pageNumber,
                pageSize
            );
        }



        public async Task<bool> ApproveAsync(Guid id)
        {
            var promotion = await _unitOfWork.promotionRepo.GetById(id).FirstOrDefaultAsync();
            if (promotion == null || promotion.IsDeleted)
                throw new NotFoundException("Không tìm thấy khuyến mãi");

            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            promotion.IsApproved = true;
            promotion.ApprovedBy = userId; 

            await _unitOfWork.promotionRepo.Update(promotion);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<PageList<PromotionResponseDto>> GetIsDeletedAsync(QueryParam? query = null)
        {

            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var source = _unitOfWork.promotionRepo.GetAll()
                .Include(p => p.Products)
                .Where(p => p.IsDeleted) 
                .AsQueryable();

            // Tìm kiếm (theo tiêu đề hoặc mô tả)
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var search = query.Search.ToLower();
                source = source.Where(p =>
                    p.Title.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search)
                );
            }

            // Sắp xếp
            if (query?.Sorts != null && query.Sorts.Any())
            {
                foreach (var sort in query.Sorts)
                {
                    if (sort.Key.Equals("title", StringComparison.OrdinalIgnoreCase))
                    {
                        source = sort.Sort == 1 ? source.OrderBy(p => p.Title) : source.OrderByDescending(p => p.Title);
                    }
                    else if (sort.Key.Equals("startDate", StringComparison.OrdinalIgnoreCase))
                    {
                        source = sort.Sort == 1 ? source.OrderBy(p => p.StartDate) : source.OrderByDescending(p => p.StartDate);
                    }
                }
            }
            else
            {
                source = source.OrderByDescending(p => p.StartDate);
            }

            // Phân trang
            var pageNumber = query?.PageNumber ?? 1;
            var pageSize = query?.PageSize ?? 10;

            var totalCount = await source.CountAsync();
            var promotions = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<PromotionResponseDto>>(promotions);

            return new PageList<PromotionResponseDto>(mapped, totalCount, pageNumber, pageSize);
        }

        public async Task<string> RestorePromotionAsync(Guid id)
        {

            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var promotion = await _unitOfWork.promotionRepo.GetById(id).FirstOrDefaultAsync();

            if (promotion == null)
                throw new BadRequestException("Không tìm thấy khuyến mãi.");

            if (!promotion.IsDeleted)
                throw new BadRequestException("Khuyến mãi này chưa bị xóa.");

            promotion.IsDeleted = false;
            promotion.IsApproved = false;
            await _unitOfWork.promotionRepo.Update(promotion);
            await _unitOfWork.CompleteAsync();

            return "Khôi phục khuyến mãi thành công.";
        }


    }
}
