using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.OrderDetail;
using Application.Model.Product;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using static Core.Entities.Enum;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.AppService.User
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ICloudinaryService cloudinaryService,
            ILogger<ProductService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task AttachPromotionsAsync(IEnumerable<ProductResponseDto> products)
        {
            if (products == null || !products.Any())
                return;

            var productIds = products.Select(p => p.Id).ToList();

            var activePromotions = await _unitOfWork.promotionRepo.GetAll()
                .Include(p => p.Products)
                .Where(p => !p.IsDeleted && p.IsApproved &&
                            p.StartDate <= DateTime.UtcNow &&
                            p.EndDate >= DateTime.UtcNow &&
                            p.Products.Any(pr => productIds.Contains(pr.Id)))
                .ToListAsync();

            foreach (var product in products)
            {
                var promo = activePromotions.FirstOrDefault(p => p.Products.Any(pr => pr.Id == product.Id));
                if (promo != null)
                {
                    product.Promotion = new PromotionDto
                    {
                        Title = promo.Title,
                        DiscountPercent = promo.DiscountPercent ?? 0,
                        EndDate = promo.EndDate
                    };
                }
            }
        }



        // ======================
        // CREATE
        // ======================
        public async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto createProduct)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = createProduct.ProductName,
                Description = createProduct.Description ?? "",
                Price = createProduct.Price,
                IdCategory = createProduct.IdCategory,
                IdSupplier = createProduct.IdSupplier,
                ProductStatus = TrangThaiSanPham.HetHang,
                Quantity = 0
            };

            // Upload ảnh
            if (createProduct.Images != null && createProduct.Images.Any())
            {
                var urls = new List<string>();
                foreach (var file in createProduct.Images)
                {
                    var url = await _cloudinaryService.UploadImageAsync(file);
                    urls.Add(url);
                }
                product.Image = JsonConvert.SerializeObject(urls);
            }

            await _unitOfWork.productRepo.Add(product);
            await _unitOfWork.CompleteAsync();

            var created = await _unitOfWork.productRepo.GetAll()
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            var mapped = _mapper.Map<ProductResponseDto>(created);
            await AttachPromotionsAsync(new List<ProductResponseDto> { mapped });

            return mapped;
        }


        // ======================
        // UPDATE
        // ======================
        public async Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductRequestDto update)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var product = await _unitOfWork.productRepo.GetById(id).FirstOrDefaultAsync();
            if (product == null)
                throw new BadRequestException("product_not_found");

            product.ProductName = update.ProductName ?? product.ProductName;
            product.Description = update.Description ?? product.Description;
            product.Price = update.Price > 0 ? update.Price : product.Price;
            product.IdCategory = update.IdCategory;
            product.IdSupplier = update.IdSupplier;
            product.IdPromotion = update.IdPromotion;

            if (update.Images != null && update.Images.Any())
            {
                var urls = new List<string>();
                foreach (var file in update.Images)
                {
                    var url = await _cloudinaryService.UploadImageAsync(file);
                    urls.Add(url);
                }
                product.Image = JsonConvert.SerializeObject(urls);
            }

            await _unitOfWork.productRepo.Update(product);
            await _unitOfWork.CompleteAsync();

            var updated = await _unitOfWork.productRepo.GetAll()
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            var mapped = _mapper.Map<ProductResponseDto>(updated);
            await AttachPromotionsAsync(new List<ProductResponseDto> { mapped });

            return mapped;
        }

        // ======================
        // DELETE
        // ======================
        public async Task<string> DeleteProductAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var product = await _unitOfWork.productRepo.GetById(id).FirstOrDefaultAsync();
            if (product == null)
                throw new BadRequestException("product_not_found");

            product.IsDeleted = true;
            await _unitOfWork.productRepo.Update(product);
            await _unitOfWork.CompleteAsync();

            return "product_deleted";
        }

        // ======================
        // GET ALL (ADMIN)
        // ======================
        public async Task<PageList<ProductResponseDto>> GetAllAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var products = await _unitOfWork.productRepo.GetAllWithInclude()
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            // ✅ Lấy tồn kho
            var stockDict = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.ProductId)
                .Select(g => new { g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.Key, x => x.Quantity);

            var mapped = _mapper.Map<List<ProductResponseDto>>(products);

            foreach (var item in mapped)
            {
                item.Quantity = stockDict.TryGetValue(item.Id, out var qty) ? qty : 0;
                item.ProductStatus = item.Quantity > 0
                    ? TrangThaiSanPham.ConHang
                    : TrangThaiSanPham.HetHang;
            }

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(query?.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                mapped = mapped.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
            }

            var totalCount = mapped.Count;
            var pageNumber = query?.PageNumber ?? 1;
            var pageSize = query?.PageSize ?? 20;

            var items = mapped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var p = new PageList<ProductResponseDto>(items, totalCount, pageNumber, pageSize);

            await AttachPromotionsAsync(p.Items);

            return p;
        }


        // ======================
        // GET ALL (CLIENT)
        // ======================
        public async Task<PageList<ProductResponseDto>> GetAllAsyncClient(ProductFilterParam filter)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Max(1, filter.PageSize);

            IQueryable<Product> q = _unitOfWork.productRepo.GetAllWithInclude()
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Price > 0);

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string keyword = filter.Search.Trim().ToLower();
                q = q.Where(p => p.ProductName.ToLower().Contains(keyword));
            }

            // 🧩 Lọc theo danh mục
            if (filter.CategoryIds != null && filter.CategoryIds.Any())
                q = q.Where(p => filter.CategoryIds.Contains(p.IdCategory));

            // 💰 Lọc theo giá
            if (filter.MinPrice.HasValue)
                q = q.Where(p => p.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                q = q.Where(p => p.Price <= filter.MaxPrice.Value);

            // 📊 Đếm tổng số bản ghi
            var totalCount = await q.CountAsync();

            // 🔄 Sắp xếp
            q = (filter.SortBy?.ToLowerInvariant()) switch
            {
                "price" => filter.SortAsc ? q.OrderBy(p => p.Price) : q.OrderByDescending(p => p.Price),
                "name" => filter.SortAsc ? q.OrderBy(p => p.ProductName) : q.OrderByDescending(p => p.ProductName),
                _ => q.OrderBy(p => p.ProductName)
            };

            // 📦 Lấy danh sách sản phẩm phân trang
            var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var mapped = _mapper.Map<List<ProductResponseDto>>(items);

            // ✅ Lấy tồn kho từ WarehouseDetail
            var productIds = items.Select(p => p.Id).ToList();
            var stockDict = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(d => !d.IsDeleted && productIds.Contains(d.ProductId))
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);

            foreach (var item in mapped)
            {
                var qty = stockDict.TryGetValue(item.Id, out var v) ? v : 0;
                item.Quantity = qty;
                item.ProductStatus = qty > 0
                    ? TrangThaiSanPham.ConHang
                    : TrangThaiSanPham.HetHang;
            }

            await AttachPromotionsAsync(mapped);


            return new PageList<ProductResponseDto>(mapped, totalCount, pageNumber, pageSize);
        }


        // ======================
        // GET BY ID
        // ======================
        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            var product = await _unitOfWork.productRepo.GetById(id)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync();

            if (product == null)
                throw new BadRequestException("product_not_found");

            var totalQuantity = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(d => d.ProductId == product.Id && !d.IsDeleted)
                .SumAsync(d => d.Quantity);

            var dto = _mapper.Map<ProductResponseDto>(product);
            dto.Quantity = totalQuantity;
            dto.ProductStatus = totalQuantity > 0 ? TrangThaiSanPham.ConHang : TrangThaiSanPham.HetHang;
            return dto;
        }

        // ======================
        // GET BY SLUG
        // ======================
        public async Task<ProductResponseDto> GetBySlugAsync(string slug)
        {
            var product = await _unitOfWork.productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted);

            if (product == null)
                throw new BadRequestException("product_not_found");

            var totalQuantity = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(d => d.ProductId == product.Id && !d.IsDeleted)
                .SumAsync(d => d.Quantity);

            var dto = _mapper.Map<ProductResponseDto>(product);
            dto.Quantity = totalQuantity;
            dto.ProductStatus = totalQuantity > 0 ? TrangThaiSanPham.ConHang : TrangThaiSanPham.HetHang;
            return dto;
        }

        // ======================
        // GET IS DELETED
        // ======================
        public async Task<PageList<ProductResponseDto>> GetIsDeletedAsync(QueryParam? query = null)
        {
            var source = _unitOfWork.productRepo.GetAll()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Promotion)
                .Where(p => p.IsDeleted);

            if (!string.IsNullOrEmpty(query?.Search))
            {
                var search = query.Search.ToLower();
                source = source.Where(p =>
                    p.ProductName.ToLower().Contains(search) ||
                    p.Category.CategoryName.ToLower().Contains(search));
            }

            var total = await source.CountAsync();
            var products = await source.Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 10))
                .Take(query?.PageSize ?? 10).ToListAsync();

            var mapped = _mapper.Map<List<ProductResponseDto>>(products);
            return new PageList<ProductResponseDto>(mapped, total, query?.PageNumber ?? 1, query?.PageSize ?? 10);
        }

        // ======================
        // RESTORE
        // ======================
        public async Task<string> RestoreProductAsync(Guid id)
        {
            var product = await _unitOfWork.productRepo.GetById(id).FirstOrDefaultAsync();
            if (product == null)
                throw new BadRequestException("Không tìm thấy sản phẩm.");

            product.IsDeleted = false;
            await _unitOfWork.productRepo.Update(product);
            await _unitOfWork.CompleteAsync();
            return "Khôi phục thành công";
        }

        // ======================
        // 3 PRODUCT SALE
        // ======================
        public async Task<List<ProductResponseDto>> Get3ProductSaleAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var threshold = now.AddDays(1);

            var products = await _unitOfWork.productRepo.GetAll()
                .Include(p => p.Promotion)
                .Where(p => !p.IsDeleted && p.Price > 0 &&
                            p.Promotion != null &&
                            p.Promotion.IsApproved &&
                            p.Promotion.EndDate > now &&
                            p.Promotion.EndDate <= threshold)
                .OrderBy(p => p.Promotion.EndDate)
                .Take(3)
                .ToListAsync();

            // ✅ Lấy tồn kho theo WarehouseDetail
            var stockDict = await _unitOfWork.warehouseDetailRepo.GetAll()
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);

            var mapped = _mapper.Map<List<ProductResponseDto>>(products);

            foreach (var item in mapped)
            {
                var totalQuantity = stockDict.ContainsKey(item.Id) ? stockDict[item.Id] : 0;
                item.Quantity = totalQuantity;
                item.ProductStatus = totalQuantity > 0 ? TrangThaiSanPham.ConHang : TrangThaiSanPham.HetHang;
            }

            return mapped.Where(p => p.ProductStatus == TrangThaiSanPham.ConHang).ToList();
        }
    }
}
