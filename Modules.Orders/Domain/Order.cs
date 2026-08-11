using Base.Domain;

namespace Modules.Orders.Domain;

public class Order : BaseEntityAppUser
{
    public Guid RestaurantId { get; set; }
    public string RestaurantNameSnapshot { get; set; } = default!;

    public string OrderStatus { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PickupTime { get; set; }
    public LangStr? Summary { get; set; } = default!;

    public ICollection<OrderRow> OrderRows { get; set; } = new List<OrderRow>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
