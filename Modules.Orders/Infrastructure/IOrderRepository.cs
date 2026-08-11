using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal interface IOrderRepository
{
    Task<List<Order>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Order?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Order?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(Order entity);
    void Remove(Order entity);
}
