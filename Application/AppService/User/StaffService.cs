using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.User;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public StaffService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<PageList<UserProfileDto>> GetAllStaffAsync(QueryParam queryParam = null)
        {
            var query = _unitOfWork.userRepo
            .GetAll()
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.Roles.Any(ur => ur.Role.Name != "customer")) // Chỉ lấy nhân viên
            .AsNoTracking();



            var pageNumber = queryParam?.PageNumber ?? 1;
            var pageSize = queryParam?.PageSize ?? 10;

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserProfileDto>>(users);

            return new PageList<UserProfileDto>(
                userDtos,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<UserProfileDto> GetStaffByIdAsync(Guid staffId)
        {
            var user = await _unitOfWork.userRepo
            .GetAll()
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .Where(u => u.Roles.Any(ur => ur.Role.Name != "customer")) // chỉ lấy nhân viên
            .FirstOrDefaultAsync(u => u.Id == staffId);


            if (user == null)
                throw new BadRequestException("Không tìm thấy nhân viên.");

            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<string> LockStaffAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var staff = await _unitOfWork.userRepo.GetById(id).FirstOrDefaultAsync();
            if (staff == null)
            {
                throw new BadRequestException("Account not found ");
            }
            staff.IsDeleted = true;
            await _unitOfWork.userRepo.Update(staff);
            await _unitOfWork.CompleteAsync();
            return " Đã khóa tài khoản người dùng";
        }

        public async Task<string> RestoreStaffAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var staff = await _unitOfWork.userRepo.GetAll()
                           .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

            if (staff == null)
            {
                throw new BadRequestException("Account not found ");
            }
            staff.IsDeleted = false;
            await _unitOfWork.userRepo.Update(staff);
            await _unitOfWork.CompleteAsync();
            return " Đã khôi phục tài khoản người dùng";
        }
    }
}
