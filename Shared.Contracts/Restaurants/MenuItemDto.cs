namespace Shared.Contracts.Restaurants;

public class MenuItemDto
{
    public Guid Id { get; set; }
    public Guid MenuCategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Summary { get; set; }
    public MenuItemCategoryDto? MenuCategory { get; set; }
}

public class MenuItemCategoryDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
