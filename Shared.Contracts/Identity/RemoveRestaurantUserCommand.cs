using MediatR;

namespace Shared.Contracts.Identity;

public sealed record RemoveRestaurantUserCommand(Guid Id, Guid AppUserId = default) : IRequest<bool>;
