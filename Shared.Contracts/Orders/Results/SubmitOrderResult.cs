namespace Shared.Contracts.Orders.Results;

public sealed record SubmitOrderResult(
    Guid OrderId,
    decimal TotalAmount);
