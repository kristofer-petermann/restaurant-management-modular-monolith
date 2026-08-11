using Base.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Modules.Identity.Domain;

public class AppRole : IdentityRole<Guid>, IBaseEntity
{
    public ICollection<AppUserRole>? AppUserRoles { get; set; }
}
