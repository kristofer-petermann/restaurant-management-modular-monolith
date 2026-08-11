using Shared.Contracts.Orders;

namespace Modules.Orders.Application.Services;

internal interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> AllAsync(Guid appUserId = default);
    Task<PaymentDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(PaymentDto entity, Guid appUserId = default);
    Task<PaymentDto> UpdateAsync(PaymentDto entity, Guid appUserId = default);
    void Remove(PaymentDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
