using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Supplier;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.AppService.User
{
    public class SupplierService : ISupplierService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SupplierService(
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

        public async Task<SupplierResponseDto> CreateSupplierAsync(SupplierRequestDto createSupplier)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var exists = await _unitOfWork.supplierRepo
                .GetAll()
                .Where(p => p.Email == createSupplier.Email && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (exists != null) throw new BadRequestException($"Đã tồn tại nhà cung cấp {exists.SupplierName} có email ");

            var supplier = new Supplier
            {
                SupplierName = createSupplier.SupplierName,
                Address= createSupplier.Address,
                Phone = createSupplier.Phone,
                Email = createSupplier.Email,
                Note = createSupplier.Note,
                CreatedBy = userId.ToString(),
                CreatedOn = DateTime.UtcNow
            };         

            await _unitOfWork.supplierRepo.Add(supplier);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SupplierResponseDto>(supplier);
        }

        public async Task<string> DeleteSupplierAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var supplier = await _unitOfWork.supplierRepo.GetById(id).FirstOrDefaultAsync();
            if (supplier == null)
                throw new BadRequestException("Supplier not found");

            supplier.IsDeleted = true;
            supplier.UpdatedBy = userId.ToString();
            supplier.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.supplierRepo.Update(supplier);
            await _unitOfWork.CompleteAsync();

            return "Đã xóa nhà cung cấp";
        }

        public async Task<PageList<SupplierResponseDto>> GetAllAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var supplier =  _unitOfWork.supplierRepo.GetAll()
                        .Where(n => !n.IsDeleted);            
            // Lọc theo search nếu có
            if (!string.IsNullOrEmpty(query?.Search))
            {
                supplier = supplier.Where(n => n.SupplierName.ToLower().Contains(query.Search));
            }
          
            var totalCount = await supplier.CountAsync();

            // Phân trang
            var supplierList = await supplier
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                .Take(query?.PageSize ?? 1000)
                .ToListAsync();

            var items = _mapper.Map<List<SupplierResponseDto>>(supplierList);
            foreach (var n in items)
            {
                // Xử lý CreatedBy
                if (!string.IsNullOrEmpty(n.CreatedBy))
                {
                    if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                    {
                        n.CreatedbyStr = _unitOfWork.userRepo
                            .GetById(createdByGuid)
                            .FirstOrDefault()?.FullName ?? "system";
                    }
                    else
                    {
                        // Nếu không phải GUID (ví dụ "admin")
                        n.CreatedbyStr = n.CreatedBy;
                    }
                }
                else
                {
                    n.CreatedbyStr = "system";
                }
                // Xử lý UpdatedBy
                if (!string.IsNullOrEmpty(n.UpdatedBy))
                {
                    if (Guid.TryParse(n.UpdatedBy, out var updatedByGuid))
                    {
                        n.UpdatedbyStr = _unitOfWork.userRepo
                            .GetById(updatedByGuid)
                            .FirstOrDefault()?.FullName ?? "system";
                    }
                    else
                    {
                        n.UpdatedbyStr = n.UpdatedBy;
                    }
                }
                else
                {
                    n.UpdatedbyStr = "system";
                }
            }
            return new PageList<SupplierResponseDto>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 1000);
        }
        public async Task<PageList<SupplierResponseDto>> GetAllAsyncDelete(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var supplier = _unitOfWork.supplierRepo.GetAll()
                        .Where(n => n.IsDeleted);
            // Lọc theo search nếu có
            if (!string.IsNullOrEmpty(query?.Search))
            {
                supplier = supplier.Where(n => n.SupplierName.ToLower().Contains(query.Search));
            }
            var totalCount = await supplier.CountAsync();
            // Phân trang
            var supplierList = await supplier
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 20))
                .Take(query?.PageSize ?? 20)
                .ToListAsync();

            var items = _mapper.Map<List<SupplierResponseDto>>(supplierList);
            foreach (var n in items)
            {
                // Xử lý CreatedBy
                if (!string.IsNullOrEmpty(n.CreatedBy))
                {
                    if (Guid.TryParse(n.CreatedBy, out var createdByGuid))
                    {
                        n.CreatedbyStr = _unitOfWork.userRepo
                            .GetById(createdByGuid)
                            .FirstOrDefault()?.FullName ?? "system";
                    }
                    else
                    {
                        // Nếu không phải GUID (ví dụ "admin")
                        n.CreatedbyStr = n.CreatedBy;
                    }
                }
                else
                {
                    n.CreatedbyStr = "system";
                }
                // Xử lý UpdatedBy
                if (!string.IsNullOrEmpty(n.UpdatedBy))
                {
                    if (Guid.TryParse(n.UpdatedBy, out var updatedByGuid))
                    {
                        n.UpdatedbyStr = _unitOfWork.userRepo
                            .GetById(updatedByGuid)
                            .FirstOrDefault()?.FullName ?? "system";
                    }
                    else
                    {
                        n.UpdatedbyStr = n.UpdatedBy;
                    }
                }
                else
                {
                    n.UpdatedbyStr = "system";
                }
            }
            return new PageList<SupplierResponseDto>(items, totalCount, query?.PageNumber ?? 1, query?.PageSize ?? 1000);
        }
        public async Task<SupplierResponseDto> GetByIdAsync(Guid id)
        {
            // Kiểm tra id hợp lệ
            if (id == Guid.Empty)
                throw new BadRequestException("Id không hợp lệ");

            // Lấy nhà cung cấp chưa bị xóa
            var supplier = await _unitOfWork.supplierRepo.GetAll()
                .Where(n => n.Id == id && !n.IsDeleted)
                .FirstOrDefaultAsync();

            if (supplier == null)
                throw new BadRequestException("Không tìm thấy nhà cung cấp");

            // Map sang DTO
            var supplierDto = _mapper.Map<SupplierResponseDto>(supplier);

            // Gán thông tin người tạo / người cập nhật
            if (!string.IsNullOrEmpty(supplierDto.CreatedBy))
            {
                if (Guid.TryParse(supplierDto.CreatedBy, out var createdGuid))
                {
                    var createdUser = _unitOfWork.userRepo.GetById(createdGuid).FirstOrDefault();
                    supplierDto.CreatedbyStr = createdUser?.FullName ?? "system";
                }
                else
                {
                    supplierDto.CreatedbyStr = supplierDto.CreatedBy; // Nếu là "admin" hay "system"
                }
            }
            else
            {
                supplierDto.CreatedbyStr = "system";
            }

            // Gán thông tin người cập nhật
            if (!string.IsNullOrEmpty(supplierDto.UpdatedBy))
            {
                if (Guid.TryParse(supplierDto.UpdatedBy, out var updatedGuid))
                {
                    var updatedUser = _unitOfWork.userRepo.GetById(updatedGuid).FirstOrDefault();
                    supplierDto.UpdatedbyStr = updatedUser?.FullName ?? "system";
                }
                else
                {
                    supplierDto.UpdatedbyStr = supplierDto.UpdatedBy;
                }
            }
            else
            {
                supplierDto.UpdatedbyStr = "system";
            }

            return supplierDto;
        }


        public async Task RestoreSupplierAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            // Lấy danh sách bản tin đang bị xóa theo danh sách id truyền vào
            var supplierList = await _unitOfWork.supplierRepo
                .GetAll()
                .Where(n => n.Id == id && n.IsDeleted)
                .ToListAsync();

            if (supplierList == null || !supplierList.Any())
                return;

            foreach (var supplier in supplierList)
            {
                supplier.IsDeleted = false;              // Khôi phục
                supplier.UpdatedBy = userId.ToString();            // Ghi nhận người khôi phục
                supplier.UpdatedOn = DateTime.UtcNow;   // Ghi nhận thời gian
            }

            await _unitOfWork.CompleteAsync();

        }
        public async Task<SupplierResponseDto> UpdateSupplierAsync(Guid supplierId, SupplierRequestDto updateSupplier)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var supplier = await _unitOfWork.supplierRepo.GetAll()
                .Where(n => n.Id == supplierId && !n.IsDeleted)
                .FirstOrDefaultAsync();

            if (supplier == null)
                throw new BadRequestException("Supplier not found");

            //Cập nhật nếu có giá trị mới, còn null thì giữ nguyên
            supplier.SupplierName = string.IsNullOrWhiteSpace(updateSupplier.SupplierName) ? supplier.SupplierName : updateSupplier.SupplierName;
            supplier.Address = string.IsNullOrWhiteSpace(updateSupplier.Address) ? supplier.Address : updateSupplier.Address;
            supplier.Phone = string.IsNullOrWhiteSpace(updateSupplier.Phone) ? supplier.Phone : updateSupplier.Phone;
            supplier.Email = string.IsNullOrWhiteSpace(updateSupplier.Email) ? supplier.Email : updateSupplier.Email;
            supplier.Note = string.IsNullOrWhiteSpace(updateSupplier.Note) ? supplier.Note : updateSupplier.Note;
            supplier.UpdatedBy = userId.ToString();
            supplier.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            // Ánh xạ sang DTO
            var response = _mapper.Map<SupplierResponseDto>(supplier);

            //Xử lý thông tin người tạo
            if (!string.IsNullOrEmpty(response.CreatedBy))
            {
                if (Guid.TryParse(response.CreatedBy, out var createdByGuid))
                {
                    response.CreatedbyStr = _unitOfWork.userRepo
                        .GetById(createdByGuid)
                        .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    response.CreatedbyStr = response.CreatedBy; 
                }
            }
            else
            {
                response.CreatedbyStr = "system";
            }
            //Xử lý thông tin người cập nhật
            if (!string.IsNullOrEmpty(response.UpdatedBy))
            {
                if (Guid.TryParse(response.UpdatedBy, out var updatedByGuid))
                {
                    response.UpdatedbyStr = _unitOfWork.userRepo
                        .GetById(updatedByGuid)
                        .FirstOrDefault()?.FullName ?? "system";
                }
                else
                {
                    response.UpdatedbyStr = response.UpdatedBy;
                }
            }
            else
            {
                response.UpdatedbyStr = "system";
            }

            return response;
        }
    }
}
