using Shared.Contracts.Orders;

namespace Modules.Orders.Application.Services;

internal interface IOrderService
{
    Task<IEnumerable<OrderDto>> AllAsync(Guid appUserId = default);
    Task<OrderDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(OrderDto entity, Guid appUserId = default);
    Task<OrderDto> UpdateAsync(OrderDto entity, Guid appUserId = default);
    void Remove(OrderDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
