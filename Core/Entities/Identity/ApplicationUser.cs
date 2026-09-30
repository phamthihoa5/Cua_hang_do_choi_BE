using Core.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Reflection;
using static Core.Entities.Enum;

namespace Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditedEntity
    {
        // Hồ sơ cơ bản
        public string? FullName { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }

        // Phân loại nhân viên
        public StaffType StaffType { get; set; } = StaffType.None;

        // Liên kết
        public ICollection<UserRole> Roles { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }

        [DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }
}
