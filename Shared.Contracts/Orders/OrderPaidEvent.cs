using MediatR;

namespace Shared.Contracts.Orders;

public sealed record OrderPaidEvent(Guid OrderId, Guid PaymentId, decimal Amount, DateTime PaidAtUtc) : INotification;
