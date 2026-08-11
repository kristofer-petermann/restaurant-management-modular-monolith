using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class GetRestaurantSummaryQueryHandler : IRequestHandler<GetRestaurantSummaryQuery, RestaurantSummaryResponse?>
{
    private readonly RestaurantsDbContext _dbContext;

    public GetRestaurantSummaryQueryHandler(RestaurantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RestaurantSummaryResponse?> Handle(GetRestaurantSummaryQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _dbContext.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant == null) return null;

        return new RestaurantSummaryResponse(
            restaurant.Id,
            restaurant.RestaurantName,
            restaurant.Summary?.ToString()
        );
    }
}
