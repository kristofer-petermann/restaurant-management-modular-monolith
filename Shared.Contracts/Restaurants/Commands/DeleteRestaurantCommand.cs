using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteRestaurantCommand(Guid RestaurantId, Guid AppUserId = default) : IRequest<bool>;
