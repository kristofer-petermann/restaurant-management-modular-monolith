using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IMenuCategoryRepository
{
    Task<List<MenuCategory>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<MenuCategory?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<MenuCategory?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuCategory?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(MenuCategory entity);
    void Remove(MenuCategory entity);
}
