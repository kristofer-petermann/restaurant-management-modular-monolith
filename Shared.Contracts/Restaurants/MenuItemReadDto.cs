namespace Shared.Contracts.Restaurants;

public class MenuItemReadDto
{
    public Guid Id { get; set; }
    public Guid MenuCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Summary { get; set; }
    public string? MenuCategoryName { get; set; }
}
