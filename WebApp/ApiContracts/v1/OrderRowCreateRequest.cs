namespace WebApp.ApiContracts.v1;

public class OrderRowCreateRequest
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Summary { get; set; }
}

