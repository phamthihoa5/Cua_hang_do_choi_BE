using Core.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("Permission")]
    public class Permission : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;   // Ví dụ: ORDER_CREATE, PRODUCT_VIEW

        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;   // Ví dụ: "Tạo đơn hàng"

        public string? Description { get; set; }

        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public ICollection<StaffTypePermission> StaffTypePermissions { get; set; } = new List<StaffTypePermission>();
    }
}
