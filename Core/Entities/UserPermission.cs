using Core.Common;
using Core.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    [Table("UserPermission")]
    public class UserPermission : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public bool IsGranted { get; set; } = true;
        public DateTime GrantedOn { get; set; } = DateTime.UtcNow;

        public Guid? GrantedById { get; set; }
        [ForeignKey(nameof(GrantedById))]
        public ApplicationUser? GrantedBy { get; set; }
    }
}
