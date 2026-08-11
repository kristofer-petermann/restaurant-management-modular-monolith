using Base.Domain;

namespace Modules.Restaurants.Domain;

public class RestaurantUser : BaseEntityAppUser
{
    public Guid RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public RestaurantUserProfile? User { get; set; }
}
