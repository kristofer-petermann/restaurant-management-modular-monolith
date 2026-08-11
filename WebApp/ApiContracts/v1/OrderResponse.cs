namespace WebApp.ApiContracts.v1;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string OrderStatus { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PickupTime { get; set; }
    public string? Summary { get; set; }
}

