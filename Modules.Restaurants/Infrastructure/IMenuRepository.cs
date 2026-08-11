using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IMenuRepository
{
    Task<List<Menu>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Menu?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Menu?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Menu?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(Menu entity);
    void Remove(Menu entity);
}
