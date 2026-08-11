using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal interface IOrderRowRepository
{
    Task<List<OrderRow>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<OrderRow?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<OrderRow?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderRow?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(OrderRow entity);
    void Remove(OrderRow entity);
}
