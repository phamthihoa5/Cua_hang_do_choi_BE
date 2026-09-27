using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Warehouse;
using Application.Model.WarehouseDetail;
using AutoMapper;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<WarehouseService> _logger;
        private readonly IMapper _mapper;

        public WarehouseService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<WarehouseService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseRequestDto request)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            if (request.Details == null || !request.Details.Any())
                throw new BadRequestException("Chi tiết kho hàng không được để trống");

            //  kho hàng mới
            var warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                DateEntered = request.DateEntered,
                TotalPrice = 0,
                Status = TrangThaiKhoHang.ConHang,
                CreatedBy = userId.ToString(),
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };
            await _unitOfWork.warehouseRepo.Add(warehouse);
            decimal totalPrice = 0;

            //Thêm các sản phẩm chi tiết trong kho
            foreach (var item in request.Details)
            {
                var product = await _unitOfWork.productRepo.GetById(item.ProductId).FirstOrDefaultAsync();
                if (product == null || product.IsDeleted)
                    throw new NotFoundException($"Không tìm thấy sản phẩm có ID: {item.ProductId}");

                // Tạo chi tiết kho
                var detail = new WarehouseDetail
                {
                    Id = Guid.NewGuid(),
                    WarehouseId = warehouse.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ImportPrice = item.ImportPrice,
                    CreatedBy = userId.ToString(),
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.warehouseDetailRepo.Add(detail);

                //Cập nhật tồn kho sản phẩm
                product.Quantity += item.Quantity;
                product.ProductStatus = product.Quantity > 0 ? TrangThaiSanPham.ConHang : TrangThaiSanPham.HetHang;

                await _unitOfWork.productRepo.Update(product);

                // Tính tổng tiền kho
                totalPrice += item.Quantity * item.ImportPrice;
            }

            //Cập nhật tổng tiền cho kho
            warehouse.TotalPrice = totalPrice;

            await _unitOfWork.warehouseRepo.Update(warehouse);
            await _unitOfWork.CompleteAsync();

            //Lấy lại dữ liệu đầy đủ để trả về
            var createdWarehouse = await _unitOfWork.warehouseRepo.GetAll()
                .Include(w => w.WarehouseDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(w => w.Id == warehouse.Id);

            return _mapper.Map<WarehouseResponseDto>(createdWarehouse);
        }
        public async Task<PageList<WarehouseResponseDto>> GetAllAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouses = _unitOfWork.warehouseRepo.GetAll()
                            .Include(w => w.WarehouseDetails)
                            .ThenInclude(d => d.Product)
                            .ThenInclude(s => s.Supplier)
                            .Where(w => !w.IsDeleted);

            if (!string.IsNullOrEmpty(query?.Search))
            {
                string search = query.Search.ToLower();
                warehouses = warehouses.Where(w => w.WarehouseDetails
                    .Any(d => d.Product.ProductName.ToLower().Contains(search)));
            }

            int total = await warehouses.CountAsync();

            var data = await warehouses
                .OrderByDescending(w => w.DateEntered)
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 10))
                .Take(query?.PageSize ?? 10)
                .ToListAsync();

            // AutoMapper sẽ map cả WarehouseDetails -> Details
            var mapped = _mapper.Map<List<WarehouseResponseDto>>(data);

            return new PageList<WarehouseResponseDto>(mapped, total, query?.PageNumber ?? 1, query?.PageSize ?? 10);
        }

        public async Task<WarehouseResponseDto> GetByIdAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouse = await _unitOfWork.warehouseRepo.GetAll()
                .Include(w => w.WarehouseDetails)
                    .ThenInclude(d => d.Product)
                    .ThenInclude(s => s.Supplier)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

            if (warehouse == null)
                throw new NotFoundException("Không tìm thấy phiếu nhập kho");

            return _mapper.Map<WarehouseResponseDto>(warehouse);
        }
        public async Task<string> DeleteWarehouseAsync(Guid id, bool force = false)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouse = await _unitOfWork.warehouseRepo.GetAll()
                .Include(w => w.WarehouseDetails)
                .ThenInclude(p => p.Product)
                .ThenInclude(s => s.Supplier)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
            if (warehouse == null)
                throw new NotFoundException("Không tìm thấy phiếu nhập kho");

            if (warehouse.WarehouseDetails.Any(d => d.Quantity > 0) && !force)
                throw new BadRequestException("Phiếu nhập này vẫn còn hàng. Cần xác nhận xóa.");

            warehouse.IsDeleted = true;
            warehouse.UpdatedOn = DateTime.UtcNow;
            warehouse.UpdatedBy = userIdStr;

            foreach (var detail in warehouse.WarehouseDetails)
            {
                detail.IsDeleted = true;
                await _unitOfWork.warehouseDetailRepo.Update(detail);
            }
            await _unitOfWork.warehouseRepo.Update(warehouse);
            await _unitOfWork.CompleteAsync();
            return "Đã xóa phiếu nhập kho thành công.";
        }
        public async Task RestoreWarehouseAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouse = await _unitOfWork.warehouseRepo.GetById(id).FirstOrDefaultAsync();
            if (warehouse == null)
                throw new NotFoundException("Không tìm thấy phiếu nhập kho");
            warehouse.IsDeleted = false;
            warehouse.UpdatedOn = DateTime.UtcNow;
            warehouse.UpdatedBy = userIdStr;
            await _unitOfWork.warehouseRepo.Update(warehouse);
            // khôi phục chi tiết sản phẩm của kho
            var details = _unitOfWork.warehouseDetailRepo.GetByWarehouseId(id);
            foreach(var detail in details)
            {
                detail.IsDeleted = false;
                _unitOfWork.warehouseDetailRepo.Update(detail);
                // Cập nhật lại số lượng sản phẩm
                var product = await _unitOfWork.productRepo.GetAll()
                              .FirstOrDefaultAsync(p => p.Id == detail.ProductId);
                if (product != null)
                {
                    product.Quantity += detail.Quantity; 
                    await _unitOfWork.productRepo.Update(product);
                }
            }
            await _unitOfWork.CompleteAsync();
        }
        public async Task<WarehouseResponseDto> UpdateWarehouseAsync(Guid id, WarehouseRequestDto request)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouse = await _unitOfWork.warehouseRepo.GetAll()
                            .Include(w => w.WarehouseDetails)
                            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
            if(warehouse == null)
            {
                throw new BadRequestException("Không tìm thấy kho hàng");
            }
            warehouse.UpdatedOn = DateTime.UtcNow;
            warehouse.UpdatedBy = userIdStr;
            warehouse.DateEntered = request.DateEntered;
            // Duyệt qua danh sách sản phẩm  trong request
            foreach (var detailDto in request.Details)
            {
                var existingDetail = warehouse.WarehouseDetails
                    .FirstOrDefault(d => d.ProductId == detailDto.ProductId);
                if (existingDetail != null)
                {
                    //Sửa sản phẩm có sẵn
                    existingDetail.Quantity = detailDto.Quantity;
                    existingDetail.ImportPrice = detailDto.ImportPrice;
                    existingDetail.UpdatedOn = DateTime.UtcNow;
                    existingDetail.UpdatedBy = userIdStr;
                    await _unitOfWork.warehouseDetailRepo.Update(existingDetail);
                }
                else
                {
                    // Thêm sản phẩm mới vào kho
                    var newDetail = new Core.Entities.WarehouseDetail
                    {
                        Id = Guid.NewGuid(),
                        WarehouseId = warehouse.Id,
                        ProductId = detailDto.ProductId,                        
                        Quantity = detailDto.Quantity,
                        ImportPrice = detailDto.ImportPrice,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userIdStr
                    };
                    await _unitOfWork.warehouseDetailRepo.Add(newDetail);
                }
            }

            //Xóa các sản phẩm không còn trong request
            var detailsToRemove = warehouse.WarehouseDetails
                .Where(d => !request.Details.Any(r => r.ProductId == d.ProductId))
                .ToList();
            // duyệt qua vòng for từng snar phẩm trong chi tiết
            foreach (var removeDetail in detailsToRemove)
            {
                removeDetail.IsDeleted = true;
                removeDetail.UpdatedOn = DateTime.UtcNow;
                removeDetail.UpdatedBy = userIdStr;
                await _unitOfWork.warehouseDetailRepo.Update(removeDetail);
            }
            // Tính lại tổng tiền của kho
            warehouse.TotalPrice = warehouse.WarehouseDetails
                .Where(d => !d.IsDeleted)
                .Sum(d => d.Quantity * d.ImportPrice);

            await _unitOfWork.warehouseRepo.Update(warehouse);
            await _unitOfWork.CompleteAsync();
            var response = _mapper.Map<WarehouseResponseDto>(warehouse);
            return response;
        }
        public async Task<PageList<WarehouseResponseDto>> GetAllDeleteAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var warehouses = _unitOfWork.warehouseRepo.GetAll()
                            .Include(w => w.WarehouseDetails)
                                .ThenInclude(d => d.Product)
                            .Where(w => w.IsDeleted);

            if (!string.IsNullOrEmpty(query?.Search))
            {
                string search = query.Search.ToLower();
                warehouses = warehouses.Where(w => w.WarehouseDetails
                    .Any(d => d.Product.ProductName.ToLower().Contains(search)));
            }

            int total = await warehouses.CountAsync();
            var data = await warehouses
                .OrderByDescending(w => w.DateEntered)
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 10))
                .Take(query?.PageSize ?? 10)
                .ToListAsync();

            // AutoMapper sẽ map cả WarehouseDetails -> Details
            var mapped = _mapper.Map<List<WarehouseResponseDto>>(data);
            return new PageList<WarehouseResponseDto>(mapped, total, query?.PageNumber ?? 1, query?.PageSize ?? 10);
        }
    }
}
