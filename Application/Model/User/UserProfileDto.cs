using System.Reflection;
using static Core.Entities.Enum;

namespace Application.Model.User
{
    /// <summary>
    /// DTO hiển thị thông tin hồ sơ người dùng
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// ID của user (Guid từ Identity)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đăng nhập (username)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email của user
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Quốc gia
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Ảnh đại diện (link online)
        /// </summary>
        //public string? AvatarUrl { get; set; }

        /// <summary>
        /// Ảnh lưu cục bộ (nếu có upload trực tiếp)
        /// </summary>
        public string? LocalPicture { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        public Gender? Gender { get; set; }


        /// <summary>
        /// Danh sách vai trò
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Lần đăng nhập gần nhất
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Trạng thái tài khoản
        /// </summary>
        public StaffType StaffType { get; set; }

        /// <summary>
        /// Đánh dấu xóa mềm
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Thông tin audit
        /// </summary>
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    /// <summary>
    /// Dùng khi muốn trả danh sách Role riêng
    /// </summary>
    public class RoleDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
