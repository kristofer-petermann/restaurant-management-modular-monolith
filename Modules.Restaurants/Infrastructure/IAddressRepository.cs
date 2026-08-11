using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IAddressRepository
{
    Task<List<Address>> GetAllAsync(Guid appUserId = default, IEnumerable<Guid>? restaurantIds = null, CancellationToken cancellationToken = default);
    Task<Address?> FindByIdAsync(Guid id, Guid appUserId = default, IEnumerable<Guid>? restaurantIds = null, CancellationToken cancellationToken = default);
    Task<Address?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Address?> FindForDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Domain.Restaurant>> GetRestaurantsAsync(IEnumerable<Guid>? allowedRestaurantIds = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AppUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    bool AppUserExists(Guid userId);
    void AddAppUser(RestaurantUserProfile appUser);
    void Add(Address entity);
    void Remove(Address entity);
}
