namespace Shared.Contracts.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PickupTime { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string RestaurantNameSnapshot { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public OrderUserDto? User { get; set; }
    public OrderRestaurantDto? Restaurant { get; set; }
}

public class OrderUserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
}

public class OrderRestaurantDto
{
    public Guid Id { get; set; }
    public string? RestaurantName { get; set; }
}
