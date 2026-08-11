using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class ValidateMenuItemsQueryHandler : IRequestHandler<ValidateMenuItemsQuery, bool>
{
    private readonly RestaurantsDbContext _dbContext;

    public ValidateMenuItemsQueryHandler(RestaurantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(ValidateMenuItemsQuery request, CancellationToken cancellationToken)
    {
        if (request.MenuItemIds.Count == 0) return false;

        var requestedIds = request.MenuItemIds.Distinct().ToList();
        var foundCount = await _dbContext.MenuItems
            .AsNoTracking()
            .Where(x => requestedIds.Contains(x.Id) && x.IsAvailable)
            .Select(x => x.Id)
            .Distinct()
            .CountAsync(cancellationToken);

        return foundCount == requestedIds.Count;
    }
}
