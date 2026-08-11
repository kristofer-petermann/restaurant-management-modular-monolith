namespace WebApp.ApiContracts.v1;

public class MenuUpdateRequest
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public string? Summary { get; set; }
}

