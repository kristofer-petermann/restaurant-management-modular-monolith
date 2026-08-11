using Base.Domain;

namespace Modules.Restaurants.Domain;

public class Address : BaseEntity
{
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public LangStr Country { get; set; } = default!;
    public LangStr? Summary { get; set; } = default!;

    public Guid? UserId { get; set; }
    public RestaurantUserProfile? User { get; set; }

    public Guid? RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
}
