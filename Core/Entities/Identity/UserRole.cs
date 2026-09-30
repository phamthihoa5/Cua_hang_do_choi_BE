using Microsoft.AspNetCore.Identity;

namespace Core.Entities.Identity;

public class UserRole : IdentityUserRole<Guid>
{
    public ApplicationUser? User { get; set; }
    public ApplicationRole? Role { get; set; }
}