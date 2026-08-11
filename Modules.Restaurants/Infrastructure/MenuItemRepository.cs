using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class MenuItemRepository : BaseRepository<MenuItem>, IMenuItemRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public MenuItemRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MenuItem>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuItems.Include(x => x.MenuCategory).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mi => mi.MenuCategory!.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuItems.Include(x => x.MenuCategory).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mi => mi.MenuCategory!.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<MenuItem?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MenuItems.FirstOrDefaultAsync(mi => mi.Id == id, cancellationToken);
    }

    public async Task<MenuItem?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuItems.Include(x => x.MenuCategory).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mi => mi.MenuCategory!.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(MenuItem entity)
    {
        _dbContext.MenuItems.Add(entity);
    }

    public void Remove(MenuItem entity)
    {
        _dbContext.MenuItems.Remove(entity);
    }
}
