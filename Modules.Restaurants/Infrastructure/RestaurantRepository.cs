using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class RestaurantRepository : BaseRepository<Restaurant>, IRestaurantRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public RestaurantRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Restaurant>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Restaurants.AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(r => r.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Restaurant?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Restaurants.Where(r => r.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(r => r.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Restaurant?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Restaurant?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Restaurants.Where(r => r.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(r => r.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Restaurant entity)
    {
        _dbContext.Restaurants.Add(entity);
    }

    public void Remove(Restaurant entity)
    {
        _dbContext.Restaurants.Remove(entity);
    }
}
