using Core.Common;
using Microsoft.AspNetCore.Http;
using static Core.Entities.Enum;

namespace Application.Model.User
{
    /// <summary>
    /// DTO trả về khi người dùng đăng nhập thành công.
    /// </summary>
    public class UserLoginResponse
    {
        /// <summary>
        /// ID của user (dùng Guid theo Identity).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Họ và tên người dùng.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email đăng nhập.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại (nếu có).
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Giới tính (nếu có).
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Địa chỉ.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Đường dẫn ảnh đại diện (nếu ứng dụng có hỗ trợ).
        /// </summary>
        public IFormFile? AvatarUrl { get; set; }

        /// <summary>
        /// Danh sách các vai trò (Roles) mà user thuộc về.
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// Token JWT để xác thực cho các request tiếp theo.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token để xin lại AccessToken mới khi hết hạn.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian hết hạn của AccessToken (UTC).
        /// </summary>
        public DateTime ExpireAt { get; set; }
    }
}
