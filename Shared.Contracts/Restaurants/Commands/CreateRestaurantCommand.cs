using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record CreateRestaurantCommand(
    string RestaurantName,
    string? Description,
    string? Phone,
    string? Email,
    string? Summary,
    Guid AppUserId = default) : IRequest<RestaurantDto>;
