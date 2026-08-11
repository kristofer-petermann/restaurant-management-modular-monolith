namespace WebApp.ApiContracts.v1;

public class OrderCreateRequest
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<SubmitOrderItemRequest> Items { get; set; } = new();
    public string? OrderStatus { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PickupTime { get; set; }
    public string? Summary { get; set; }
}
