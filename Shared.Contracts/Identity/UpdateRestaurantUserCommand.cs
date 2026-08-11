using MediatR;

namespace Shared.Contracts.Identity;

public sealed record UpdateRestaurantUserCommand(
    Guid Id,
    Guid RestaurantId,
    Guid UserId,
    RestaurantUserRole UserRole,
    string? Role,
    string? Summary,
    Guid AppUserId = default) : IRequest<RestaurantUserReadDto?>;
