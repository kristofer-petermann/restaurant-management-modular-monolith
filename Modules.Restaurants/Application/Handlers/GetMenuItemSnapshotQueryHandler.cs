using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class GetMenuItemSnapshotQueryHandler : IRequestHandler<GetMenuItemSnapshotQuery, MenuItemSnapshotResponse?>
{
    private readonly RestaurantsDbContext _dbContext;

    public GetMenuItemSnapshotQueryHandler(RestaurantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MenuItemSnapshotResponse?> Handle(GetMenuItemSnapshotQuery request, CancellationToken cancellationToken)
    {
        var menuItem = await _dbContext.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.MenuItemId, cancellationToken);

        if (menuItem == null) return null;

        return new MenuItemSnapshotResponse(
            menuItem.Id,
            menuItem.Name.ToString(),
            menuItem.Price,
            true
        );
    }
}
