using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal interface IPaymentRepository
{
    Task<List<Payment>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Payment?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Payment?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(Payment entity);
    void Remove(Payment entity);
}
