using MediatR;

namespace Shared.Contracts.Events;

public sealed record RestaurantUserAssignedEvent(Guid RestaurantId, Guid UserId, string Role, DateTime AssignedAtUtc) : INotification;
