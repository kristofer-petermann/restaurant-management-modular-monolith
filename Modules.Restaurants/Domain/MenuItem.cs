using Base.Domain;

namespace Modules.Restaurants.Domain;

public class MenuItem : BaseEntity
{
    public Guid MenuCategoryId { get; set; }
    public MenuCategory? MenuCategory { get; set; }

    public LangStr Name { get; set; } = default!;
    public LangStr? Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; } = default!;
    public LangStr? Summary { get; set; } = default!;
}
