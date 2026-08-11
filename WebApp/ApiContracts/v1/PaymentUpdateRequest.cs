namespace WebApp.ApiContracts.v1;

public class PaymentUpdateRequest
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public DateTime? PaidAt { get; set; }
    public string? Summary { get; set; }
}

