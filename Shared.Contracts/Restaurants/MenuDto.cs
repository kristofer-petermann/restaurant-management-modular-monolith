namespace Shared.Contracts.Restaurants;

public class MenuDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public string? Summary { get; set; }
    public MenuRestaurantDto? Restaurant { get; set; }
}

public class MenuRestaurantDto
{
    public Guid Id { get; set; }
    public string? RestaurantName { get; set; }
}
