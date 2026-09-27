using Application.Common.Models;
using Application.Model.API;
using Application.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IStaffService
    {
        // Danh sách nhân viên
        Task<PageList<UserProfileDto>> GetAllStaffAsync(QueryParam query = null);
        // Lấy nhân viên theo ID
        Task<UserProfileDto> GetStaffByIdAsync(Guid staffId);
        Task<string> LockStaffAsync(Guid id);
        Task<string> RestoreStaffAsync(Guid id);
    }
}
