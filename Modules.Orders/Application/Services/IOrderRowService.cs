using Shared.Contracts.Orders;

namespace Modules.Orders.Application.Services;

internal interface IOrderRowService
{
    Task<IEnumerable<OrderRowDto>> AllAsync(Guid appUserId = default);
    Task<OrderRowDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(OrderRowDto entity, Guid appUserId = default);
    Task<OrderRowDto> UpdateAsync(OrderRowDto entity, Guid appUserId = default);
    void Remove(OrderRowDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
