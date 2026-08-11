namespace WebApp.ApiContracts.v1;

public class MenuItemResponse
{
    public Guid Id { get; set; }
    public Guid MenuCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Summary { get; set; }
}

