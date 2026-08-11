namespace Shared.Contracts.Orders;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? Summary { get; set; }
    public PaymentOrderDto? Order { get; set; }
}

public class PaymentOrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentUserDto? User { get; set; }
}

public class PaymentUserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
}
