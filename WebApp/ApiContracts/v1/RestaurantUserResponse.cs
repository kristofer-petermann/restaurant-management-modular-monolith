namespace WebApp.ApiContracts.v1;

public class RestaurantUserResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public int UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }
}
