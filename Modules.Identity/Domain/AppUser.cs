using Base.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Modules.Identity.Domain;

public class AppUser : IdentityUser<Guid>, IBaseEntity
{
    public ICollection<AppUserRole>? AppUserRoles { get; set; }
    public ICollection<RestaurantUser>? RestaurantUsers { get; set; }
    public ICollection<AppRefreshToken>? RefreshTokens { get; set; }
}
