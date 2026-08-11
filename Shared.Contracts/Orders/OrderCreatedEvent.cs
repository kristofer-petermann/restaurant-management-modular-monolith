using MediatR;

namespace Shared.Contracts.Orders;

public sealed record OrderCreatedEvent(Guid OrderId, Guid UserId, Guid RestaurantId, DateTime CreatedAtUtc) : INotification;
