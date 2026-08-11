using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record CreateMenuCommand(
    Guid RestaurantId,
    string? Name,
    bool IsActive,
    string? Summary,
    Guid AppUserId = default) : IRequest<MenuDto>;
