namespace Shared.Contracts.Restaurants;

public class MenuReadDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Summary { get; set; }
    public string? RestaurantName { get; set; }
}
