namespace WebApp.ApiContracts.v1;

public class SubmitOrderResponse
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
}
