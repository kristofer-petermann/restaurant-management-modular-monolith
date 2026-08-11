namespace Shared.Contracts.Restaurants;

public class RestaurantReadDto
{
    public Guid Id { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Summary { get; set; }
}
