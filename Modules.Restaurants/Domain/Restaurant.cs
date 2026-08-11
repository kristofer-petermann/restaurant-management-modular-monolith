using Base.Domain;

namespace Modules.Restaurants.Domain;

public class Restaurant : BaseEntity
{
    public string RestaurantName { get; set; } = default!;
    public LangStr? Description { get; set; } = default!;
    public string? Phone { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public LangStr? Summary { get; set; } = default!;

    public ICollection<RestaurantUser>? RestaurantUsers { get; set; }
    public ICollection<OpeningHour>? OpeningHours { get; set; }
    public ICollection<Address>? Addresses { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<Menu>? Menus { get; set; }
}
