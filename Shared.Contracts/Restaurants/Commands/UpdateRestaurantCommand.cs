using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateRestaurantCommand(
    Guid RestaurantId,
    string RestaurantName,
    string? Description,
    string? Phone,
    string? Email,
    string? Summary,
    Guid AppUserId = default) : IRequest<RestaurantDto?>;
