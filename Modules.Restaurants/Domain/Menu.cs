using Base.Domain;

namespace Modules.Restaurants.Domain;

public class Menu : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public LangStr Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public LangStr? Summary { get; set; } = default!;

    public ICollection<MenuCategory>? MenuCategories { get; set; }
}
