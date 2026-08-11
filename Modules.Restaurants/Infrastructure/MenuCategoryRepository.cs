using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class MenuCategoryRepository : BaseRepository<MenuCategory>, IMenuCategoryRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public MenuCategoryRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MenuCategory>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuCategories.Include(x => x.Menu).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mc => mc.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<MenuCategory?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuCategories.Include(x => x.Menu).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mc => mc.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<MenuCategory?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MenuCategories.FirstOrDefaultAsync(mc => mc.Id == id, cancellationToken);
    }

    public async Task<MenuCategory?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuCategories.Include(x => x.Menu).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(mc => mc.Menu!.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(MenuCategory entity)
    {
        _dbContext.MenuCategories.Add(entity);
    }

    public void Remove(MenuCategory entity)
    {
        _dbContext.MenuCategories.Remove(entity);
    }
}
