using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class OpeningHourRepository : BaseRepository<OpeningHour>, IOpeningHourRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public OpeningHourRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OpeningHour>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OpeningHours.Include(x => x.Restaurant).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(oh => oh.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<OpeningHour?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OpeningHours.Include(x => x.Restaurant).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(oh => oh.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<OpeningHour?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.OpeningHours.FirstOrDefaultAsync(oh => oh.Id == id, cancellationToken);
    }

    public async Task<OpeningHour?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OpeningHours.Include(x => x.Restaurant).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(oh => oh.Restaurant!.RestaurantUsers!.Any(ru => ru.UserId == appUserId));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(OpeningHour entity)
    {
        _dbContext.OpeningHours.Add(entity);
    }

    public void Remove(OpeningHour entity)
    {
        _dbContext.OpeningHours.Remove(entity);
    }
}
