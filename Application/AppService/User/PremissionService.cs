using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Permission;
using Application.Model.User;
using AutoMapper;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        // =======================================================
        // Lấy quyền của người dùng hiện tại
        // =======================================================
        public async Task<UserPermissionDto> GetUserPermissionsAsync()
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var user = await _unitOfWork.userRepo
                .GetAll()
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new BadRequestException("Không tìm thấy nhân viên.");

            var allPermissions = await _unitOfWork.permissionRepo
                .GetAllPermissions()
                .AsNoTracking()
                .ToListAsync();

            var permissionsDto = _mapper.Map<List<PermissionDto>>(allPermissions);
            var grantedIds = user.UserPermissions.Select(up => up.PermissionId).ToHashSet();

            permissionsDto.ForEach(p => p.IsGranted = grantedIds.Contains(p.Id));

            return new UserPermissionDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Permissions = permissionsDto.OrderBy(p => p.Code).ToList()
            };
        }

        // =======================================================
        // Lấy toàn bộ quyền trong hệ thống
        // =======================================================
        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _unitOfWork.permissionRepo
                .GetAllPermissions()
                .AsNoTracking()
                .OrderBy(p => p.Code)
                .ToListAsync();

            return _mapper.Map<List<PermissionDto>>(permissions);
        }

        // =======================================================
        // Cập nhật quyền cho chức vụ dựa trên quyền của nhân viên mẫu
        // =======================================================
        public async Task UpdateStaffTypePermissionsAsync(Guid staffId, AssignStaffTypeDto request)
        {
            // 1. Xác thực người cập nhật (Manager)
            var managerIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(managerIdStr, out _))
                throw new BadRequestException("invalid_manager");

            var staffType = request.StaffType;

            // 2. Kiểm tra xem StaffType có hợp lệ không
            if (!System.Enum.IsDefined(typeof(StaffType), staffType))
                throw new BadRequestException("Chức vụ không hợp lệ.");

            // 3. Lấy thông tin nhân viên theo staffId
            var staffEntity = await _unitOfWork.userRepo
                .GetAll()
                .Include(u => u.UserPermissions) // Lấy quyền đã cấp cho nhân viên
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == staffId);

            if (staffEntity == null)
                throw new BadRequestException("Không tìm thấy nhân viên mẫu.");

            // 4. Cập nhật StaffType cho nhân viên
            staffEntity.StaffType = staffType;  // Cập nhật chức vụ cho nhân viên
            await _unitOfWork.userRepo.Update(staffEntity);  // Lưu lại thay đổi cho nhân viên

            // 5. Lấy danh sách PermissionIds của nhân viên mẫu
            var permissionIds = staffEntity.UserPermissions.Select(up => up.PermissionId).ToList();

            // 6. Xóa quyền cũ của chức vụ trong bảng StaffTypePermission
            var currentPermissions = await _unitOfWork.staffTypePermissionRepo
                .GetByStaffType(staffType)
                .ToListAsync();

            if (currentPermissions.Any())
                await _unitOfWork.staffTypePermissionRepo.DeleteRangeAsync(currentPermissions);

            // 7. Thêm quyền mới từ nhân viên mẫu vào StaffTypePermission
            if (permissionIds.Any())
            {
                var newPermissions = permissionIds.Select(pid => new StaffTypePermission
                {
                    StaffType = staffType,
                    PermissionId = pid,
                    IsGranted = true // Quyền được cấp cho chức vụ
                }).ToList();

                await _unitOfWork.staffTypePermissionRepo.AddRangeAsync(newPermissions);
            }

            // 8. Lưu thay đổi vào cơ sở dữ liệu
            await _unitOfWork.CompleteAsync();
        }



        // =======================================================
        // Thêm quyền mới vào chức vụ (dùng AutoMapper)
        // =======================================================
        public async Task AddPermissionsToStaffTypeAsync(UpdateStaffTypePermissionDto update)
        {
            // 1️⃣ Xác thực người thao tác
            var managerIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(managerIdStr, out _))
                throw new BadRequestException("invalid_manager");

            // 2️⃣ Kiểm tra StaffType hợp lệ
            if (!System.Enum.IsDefined(typeof(StaffType), update.StaffType))
                throw new BadRequestException("Loại nhân viên không hợp lệ.");

            // 3️⃣ Kiểm tra danh sách quyền có dữ liệu không
            if (update.PermissionIds == null || !update.PermissionIds.Any())
                throw new BadRequestException("Danh sách quyền không được để trống.");

            var staffType = update.StaffType;

            // 4️⃣ Lấy danh sách quyền hiện có của StaffType (tránh trùng)
            var existingPermissions = await _unitOfWork.staffTypePermissionRepo
                .GetByStaffType(staffType)
                .AsNoTracking()
                .Select(x => x.PermissionId)
                .ToListAsync();

            // 5️⃣ Lọc ra các quyền mới chưa có
            var permissionIdsToAdd = update.PermissionIds
                .Where(pid => !existingPermissions.Contains(pid))
                .Distinct()
                .ToList();

            if (!permissionIdsToAdd.Any())
                return; // Không có quyền mới để thêm

            // 6️⃣ Tạo danh sách entity mới để thêm
            var newPermissions = permissionIdsToAdd.Select(pid => new StaffTypePermission
            {
                StaffType = staffType,
                PermissionId = pid,
                IsGranted = true
            }).ToList();

            // 7️⃣ Thêm và lưu
            await _unitOfWork.staffTypePermissionRepo.AddRangeAsync(newPermissions);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<List<PermissionDto>> GetAdminPermission()
        {
            // 🔹 Tìm user có role Manager
            var managerUser = await _unitOfWork.userRepo
                .GetAll()
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Role.Name == "manager"))
                .FirstOrDefaultAsync();

            if (managerUser == null)
                throw new BadRequestException("Không tìm thấy người dùng Manager.");

            // 🔹 Lấy danh sách quyền từ bảng UserPermissions
            var managerPermissions = await _unitOfWork.permissionuserRepo
                .GetAll()
                .Include(up => up.Permission)
                .Where(up => up.UserId == managerUser.Id && up.IsGranted)
                .Select(up => up.Permission)
                .AsNoTracking()
                .OrderBy(p => p.Code)
                .ToListAsync();

            var result = _mapper.Map<List<PermissionDto>>(managerPermissions);
            return result;
        }

        public async Task<List<PermissionDto>> GetPermissionsByStaffTypeAsync(StaffType staffType)
        {
            // 1️⃣ Kiểm tra nếu loại nhân viên hợp lệ
            if (!System.Enum.IsDefined(typeof(StaffType), staffType))
            {
                throw new BadRequestException($"Chức vụ '{staffType}' không hợp lệ.");
            }

            // 2️⃣ Lấy toàn bộ quyền của staffType tương ứng (với điều kiện IsGranted = true)
            var permissions = await _unitOfWork.staffTypePermissionRepo
                .GetAll()
                .Include(stp => stp.Permission) // Lấy thông tin Permission liên quan
                .Where(stp => stp.StaffType == staffType && stp.IsGranted)
                .Select(stp => stp.Permission)
                .AsNoTracking() // Tránh việc tracking trong DbContext để tăng hiệu suất
                .ToListAsync();

            // 3️⃣ Kiểm tra nếu không có quyền nào
            if (permissions == null || !permissions.Any())
            {
                throw new BadRequestException($"Không tìm thấy quyền nào cho loại nhân viên: {staffType}");
            }

            // 4️⃣ Trả về danh sách PermissionDto đã được map trong service (tránh lặp lại)
            return _mapper.Map<List<PermissionDto>>(permissions);
        }


    }
}
