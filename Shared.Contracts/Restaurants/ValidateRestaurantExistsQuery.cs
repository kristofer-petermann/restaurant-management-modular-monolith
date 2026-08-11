using MediatR;

namespace Shared.Contracts.Restaurants;

public sealed record ValidateRestaurantExistsQuery(Guid RestaurantId) : IRequest<bool>;
