namespace WebApp.ApiContracts.v1;

public class RestaurantCreateRequest
{
    public string RestaurantName { get; set; } = default!;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Summary { get; set; }
}

