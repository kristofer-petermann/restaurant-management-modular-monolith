using Base.Domain;

namespace Modules.Restaurants.Domain;

public class RestaurantUserProfile : BaseEntity
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public ICollection<RestaurantUser>? RestaurantMemberships { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<Address>? Addresses { get; set; }
}
