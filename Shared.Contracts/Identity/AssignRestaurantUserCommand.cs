using MediatR;

namespace Shared.Contracts.Identity;

public sealed record AssignRestaurantUserCommand(
    Guid RestaurantId,
    Guid UserId,
    RestaurantUserRole UserRole,
    string? Role,
    string? Summary,
    Guid AppUserId = default) : IRequest<RestaurantUserReadDto>;
