using Base.Domain;

namespace Modules.Restaurants.Domain;

public class MenuCategory : BaseEntity
{
    public Guid MenuId { get; set; }
    public Menu? Menu { get; set; }

    public LangStr Name { get; set; } = default!;
    public LangStr? Summary { get; set; } = default!;

    public ICollection<MenuItem>? MenuItems { get; set; }
}
