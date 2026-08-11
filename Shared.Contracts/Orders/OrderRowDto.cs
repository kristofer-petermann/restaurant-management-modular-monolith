namespace Shared.Contracts.Orders;

public class OrderRowDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string? MenuItemNameSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal LineTotal { get; set; }
    public string? Summary { get; set; }
    public OrderRowOrderDto? Order { get; set; }
    public OrderRowMenuItemDto? MenuItem { get; set; }

    public decimal UnitPrice
    {
        get => UnitPriceSnapshot;
        set => UnitPriceSnapshot = value;
    }
}

public class OrderRowOrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderRowUserDto? User { get; set; }
}

public class OrderRowUserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
}

public class OrderRowMenuItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
