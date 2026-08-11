using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateMenuCommand(
    Guid MenuId,
    Guid RestaurantId,
    string? Name,
    bool IsActive,
    string? Summary,
    Guid AppUserId = default) : IRequest<MenuDto?>;
