using MediatR;

namespace Shared.Contracts.Restaurants;

public sealed record GetRestaurantSummaryQuery(Guid RestaurantId) : IRequest<RestaurantSummaryResponse?>;
