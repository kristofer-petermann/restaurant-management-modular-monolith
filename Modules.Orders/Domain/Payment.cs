using Base.Domain;

namespace Modules.Orders.Domain;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public DateTime? PaidAt { get; set; }
    public LangStr? Summary { get; set; } = default!;
}
