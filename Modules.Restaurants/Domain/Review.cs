using Base.Domain;

namespace Modules.Restaurants.Domain;

public class Review : BaseEntityAppUser
{
    public RestaurantUserProfile? AppUser { get; set; }

    public Guid RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public LangStr? Summary { get; set; } = default!;
}
