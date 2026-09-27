
using Core.Entities;
using static Core.Entities.Enum;

namespace Application.Model.Permission
{
    /// <summary>
    /// DTO mô tả chi tiết 1 quyền (Permission)
    /// </summary>
    public class PermissionDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Mã quyền, ví dụ: PRODUCT_CREATE
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên hiển thị quyền, ví dụ: "Tạo sản phẩm"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái đã được cấp quyền hay chưa
        /// </summary>
        public bool IsGranted { get; set; } = false;
    }

    /// <summary>
    /// DTO chứa danh sách quyền của một người dùng
    /// </summary>
    public class UserPermissionDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Dùng khi cập nhật quyền mặc định cho một chức vụ (StaffType)
    /// </summary>
    public class UpdateStaffTypePermissionDto
    {
        /// <summary>
        /// Tên chức vụ: "Sales", "Warehouse", ...
        /// </summary>
        public StaffType StaffType { get; set; }

        /// <summary>
        /// Danh sách ID các quyền được phép
        /// </summary>
        public List<Guid> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// Dùng khi xem danh sách quyền mặc định của một chức vụ
    /// </summary>
    public class StaffTypePermissionDto
    {
        /// <summary>
        /// Loại nhân viên: Sales / Warehouse
        /// </summary>
        public StaffType StaffType { get; set; }

        /// <summary>
        /// Danh sách quyền được cấp mặc định
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Dùng khi gán loại nhân viên cho user (AssignStaffTypeAsync)
    /// </summary>
    public class AssignStaffTypeDto
    {
        /// <summary>
        /// Tên loại nhân viên: Sales / Warehouse
        /// </summary>
        public StaffType StaffType { get; set; }
    }
}
