using Core.Common;
using System.ComponentModel.DataAnnotations.Schema;
using static Core.Entities.Enum;

namespace Core.Entities
{
    [Table("StaffTypePermission")]
    public class StaffTypePermission : BaseEntity
    {
        public StaffType StaffType { get; set; }           // Sales / Warehouse

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public bool IsGranted { get; set; } = true;
    }
}
