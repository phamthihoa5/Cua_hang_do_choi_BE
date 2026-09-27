using Microsoft.AspNetCore.Http;
using static Core.Entities.Enum;

namespace Application.Model.Auth;

/// <summary>
/// Yêu cầu đăng ký tài khoản mới
/// </summary>
public class SignUpRequest
{
    /// <summary>
    /// Họ và tên
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email đăng nhập
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Giới tính
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Phản hồi sau khi đăng ký tài khoản
/// </summary>
public class SignUpResponse
{
    public bool Ok { get; set; }
    public Guid? UserId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Yêu cầu cập nhật thông tin cá nhân
/// </summary>
public class UpdateInfoRequest
{
    /// <summary>
    /// Ảnh đại diện
    /// </summary>
    public IFormFile? Avatar { get; set; }

    /// <summary>
    /// Họ và tên
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Giới tính
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; set; }
}
