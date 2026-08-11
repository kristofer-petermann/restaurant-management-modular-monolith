using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IRestaurantRepository
{
    Task<List<Restaurant>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Restaurant?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Restaurant?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Restaurant?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(Restaurant entity);
    void Remove(Restaurant entity);
}
