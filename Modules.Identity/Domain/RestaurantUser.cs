using Base.Domain;

namespace Modules.Identity.Domain;

public class RestaurantUser : BaseEntityAppUser
{
    public Guid RestaurantId { get; set; }
    public AppUser? User { get; set; }
    public EUserRoleInRestaurant UserRole { get; set; } = EUserRoleInRestaurant.Normal;
    public string? Role { get; set; } = default!;
    public LangStr? Summary { get; set; } = default!;
}
