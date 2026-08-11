using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class MenuRepository : BaseRepository<Menu>, IMenuRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public MenuRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Menu>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Menus.Include(x => x.Restaurant).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(m => m.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Menu?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Menus.Include(x => x.Restaurant).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(m => m.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Menu?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Menus.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Menu?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Menus.Include(x => x.Restaurant).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(m => m.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Menu entity)
    {
        _dbContext.Menus.Add(entity);
    }

    public void Remove(Menu entity)
    {
        _dbContext.Menus.Remove(entity);
    }
}
