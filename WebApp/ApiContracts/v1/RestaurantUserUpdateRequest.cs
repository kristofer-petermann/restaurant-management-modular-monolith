namespace WebApp.ApiContracts.v1;

public class RestaurantUserUpdateRequest
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public int UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }
}

