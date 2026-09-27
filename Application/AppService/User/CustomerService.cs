using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Customer;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        public CustomerService(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<string> AccountLockAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var customer = await _unitOfWork.userRepo.GetById(id).FirstOrDefaultAsync();
            if(customer == null)
            {
                throw new BadRequestException("Account not found ");
            }
            customer.IsDeleted = true;
            customer.UpdatedOn = DateTime.UtcNow;
            customer.UpdatedBy = userIdStr;
            await _unitOfWork.userRepo.Update(customer);
            await _unitOfWork.CompleteAsync();
            return " Đã khóa tài khoản người dùng";
        }

        public async Task<PageList<CustomerResponseDto>> GetAllCustomersAsync(QueryParam query = null)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            // Lấy danh sách customer
            var customersQuery = _unitOfWork.userRepo.GetCustomers()
                .Select(u => new CustomerResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Status = u.IsDeleted ? "Đã khóa" : "Đang hoạt động"
                });

            // Lọc theo search
            if (!string.IsNullOrEmpty(query?.Search))
            {
                string search = query.Search.ToLower();
                customersQuery = customersQuery.Where(c => c.FullName.ToLower().Contains(search));
            }
            // Đếm tổng
            var totalCount = await customersQuery.CountAsync();
            // Phân trang
            int pageNumber = query?.PageNumber ?? 1;
            int pageSize = query?.PageSize ?? 20;
            var items = await customersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Trả về PageList
            return new PageList<CustomerResponseDto>(items, totalCount, pageNumber, pageSize);
        }
        public async Task<string> RestoreAccountAsync(Guid id)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");
            var customer = await _unitOfWork.userRepo.GetAll()
                           .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
                          
            if (customer == null)
            {
                throw new BadRequestException("Account not found ");
            }
            customer.IsDeleted = false;
            customer.UpdatedOn = DateTime.UtcNow;
            customer.UpdatedBy = userIdStr;
            await _unitOfWork.userRepo.Update(customer);
            await _unitOfWork.CompleteAsync();
            return " Đã khôi phục tài khoản người dùng";
        }
    }
}
