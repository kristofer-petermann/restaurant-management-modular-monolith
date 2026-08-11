using MediatR;

namespace Shared.Contracts.Events;

public sealed record ReviewCreatedEvent(Guid ReviewId, Guid RestaurantId, Guid UserId, int Rating, DateTime CreatedAtUtc) : INotification;
