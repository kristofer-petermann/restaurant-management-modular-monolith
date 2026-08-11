using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal class AddressRepository : BaseRepository<Address>, IAddressRepository
{
    private readonly RestaurantsDbContext _dbContext;

    public AddressRepository(RestaurantsDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Address>> GetAllAsync(Guid appUserId = default, IEnumerable<Guid>? restaurantIds = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Addresses
            .Include(a => a.Restaurant)
            .Include(a => a.User)
            .AsQueryable();

        if (appUserId != default)
        {
            query = ApplyUserAddressFilter(query, appUserId, restaurantIds);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Address?> FindByIdAsync(Guid id, Guid appUserId = default, IEnumerable<Guid>? restaurantIds = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Addresses
            .Include(a => a.Restaurant)
            .Include(a => a.User)
            .Where(a => a.Id == id)
            .AsQueryable();

        if (appUserId != default)
        {
            query = ApplyUserAddressFilter(query, appUserId, restaurantIds);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Address?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public Task<Address?> FindForDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Restaurant>> GetRestaurantsAsync(IEnumerable<Guid>? allowedRestaurantIds = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Restaurants.AsQueryable();
        if (allowedRestaurantIds != null)
        {
            var ids = allowedRestaurantIds.ToList();
            query = query.Where(r => ids.Contains(r.Id));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Addresses.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public Task<bool> AppUserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<RestaurantUserProfile>().AnyAsync(u => u.Id == userId, cancellationToken);
    }

    public bool AppUserExists(Guid userId)
    {
        return _dbContext.Set<RestaurantUserProfile>().Any(u => u.Id == userId);
    }

    public void AddAppUser(RestaurantUserProfile appUser)
    {
        _dbContext.Set<RestaurantUserProfile>().Add(appUser);
    }

    public void Add(Address entity)
    {
        _dbContext.Addresses.Add(entity);
    }

    public void Remove(Address entity)
    {
        _dbContext.Addresses.Remove(entity);
    }

    private static IQueryable<Address> ApplyUserAddressFilter(
        IQueryable<Address> query,
        Guid appUserId,
        IEnumerable<Guid>? restaurantIds)
    {
        var allowedRestaurantIds = restaurantIds?.ToList();
        return query.Where(a =>
            a.UserId == appUserId ||
            (a.RestaurantId.HasValue &&
             (allowedRestaurantIds == null || allowedRestaurantIds.Contains(a.RestaurantId.Value))));
    }
}
