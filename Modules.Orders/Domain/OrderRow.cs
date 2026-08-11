using Base.Domain;

namespace Modules.Orders.Domain;

public class OrderRow : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid MenuItemId { get; set; }
    public string MenuItemNameSnapshot { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal LineTotal { get; set; }
    public LangStr? Summary { get; set; } = default!;
}
