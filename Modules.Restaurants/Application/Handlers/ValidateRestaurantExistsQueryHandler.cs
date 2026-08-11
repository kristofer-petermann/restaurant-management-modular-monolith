using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class ValidateRestaurantExistsQueryHandler : IRequestHandler<ValidateRestaurantExistsQuery, bool>
{
    private readonly RestaurantsDbContext _dbContext;

    public ValidateRestaurantExistsQueryHandler(RestaurantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> Handle(ValidateRestaurantExistsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.Restaurants
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.RestaurantId, cancellationToken);
    }
}
