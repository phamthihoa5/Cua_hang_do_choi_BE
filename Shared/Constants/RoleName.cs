namespace Shared.Constants
{
    /// <summary>
    /// Tên & Id vai trò dùng cho seeding và kiểm tra phân quyền.
    /// Lưu ý: Nếu đã seed trước đó, đổi GUID cần đồng bộ lại dữ liệu.
    /// </summary>
    public static class RoleName
    {
        // IDs dùng khi seed bảng AspNetRoles (cố định để idempotent)
        public static readonly Guid ManagerId = Guid.Parse("d25bdb1c-582b-4c2c-a875-31c2e85bd567"); // dùng lại GUID cũ
        public static readonly Guid StaffId = Guid.Parse("a7a1b2c3-d4e5-46f7-8901-23456789abcd");
        public static readonly Guid CustomerId = Guid.Parse("b5e8f1a2-3c4d-5e6f-8a90-b1c2d3e4f567");

        // Tên role (slug, dùng trong code, policy, attribute)
        public const string Manager = "manager";   // Quản lý
        public const string Staff = "staff";     // Nhân viên
        public const string Customer = "customer";  // Khách

        // Tên role đã chuẩn hoá (thường dùng cho NormalizedName trong AspNetRoles)
        public const string MANAGER = "MANAGER";
        public const string STAFF = "STAFF";
        public const string CUSTOMER = "CUSTOMER";

        // Gom nhóm tiện dùng
        public const string InternalRolesCsv = $"{Manager}, {Staff}";
        public const string AllRolesCsv = $"{Manager}, {Staff}, {Customer}";

        public static readonly string[] InternalRoles = new[] { Manager, Staff };
        public static readonly string[] All = new[] { Manager, Staff, Customer };
    }
}
