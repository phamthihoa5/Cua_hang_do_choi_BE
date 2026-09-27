using Application.Common.Models;
using Application.Model.API;
using Application.Model.Permission;
using Application.Model.User;
using static Core.Entities.Enum;

namespace Application.IService.User
{
    public interface IPermissionService
    {
        Task<UserPermissionDto> GetUserPermissionsAsync();

        Task<List<PermissionDto>> GetAllPermissionsAsync();

        Task<List<PermissionDto>> GetAdminPermission();

        Task<List<PermissionDto>> GetPermissionsByStaffTypeAsync(StaffType staffType);

        Task UpdateStaffTypePermissionsAsync(Guid StaffId, AssignStaffTypeDto request);

        Task AddPermissionsToStaffTypeAsync(UpdateStaffTypePermissionDto update);

    }
}

