using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IMenuItemRepository
{
    Task<List<MenuItem>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<MenuItem?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<MenuItem?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuItem?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(MenuItem entity);
    void Remove(MenuItem entity);
}
