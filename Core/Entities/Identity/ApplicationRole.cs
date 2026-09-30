using Microsoft.AspNetCore.Identity;

namespace Core.Entities.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ICollection<UserRole> Users { get; set; }
    public int Priority { get; set; } = 0;
}