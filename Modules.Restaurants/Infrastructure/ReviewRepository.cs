using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public ReviewRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Review>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Reviews.Include(x => x.Restaurant).Include(x => x.AppUser).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(r => r.UserId == appUserId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Review?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Reviews.Include(x => x.Restaurant).Include(x => x.AppUser).Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(r => r.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Review?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Review?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews.FirstOrDefaultAsync(x =>
            x.Id == id && (appUserId == default || x.UserId == appUserId), cancellationToken);
    }

    public void Add(Review entity)
    {
        _dbContext.Reviews.Add(entity);
    }

    public void Remove(Review entity)
    {
        _dbContext.Reviews.Remove(entity);
    }
}
