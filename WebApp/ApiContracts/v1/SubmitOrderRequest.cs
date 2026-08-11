namespace WebApp.ApiContracts.v1;

public class SubmitOrderRequest
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<SubmitOrderItemRequest> Items { get; set; } = new();
    public DateTime? PickupTime { get; set; }
    public string? Summary { get; set; }
}

public class SubmitOrderItemRequest
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}
