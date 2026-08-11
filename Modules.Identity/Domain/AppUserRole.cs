using Microsoft.AspNetCore.Identity;

namespace Modules.Identity.Domain;

public class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser User { get; set; } = default!;
    public AppRole Role { get; set; } = default!;
}
