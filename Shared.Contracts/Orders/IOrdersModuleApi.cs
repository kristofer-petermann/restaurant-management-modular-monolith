namespace Shared.Contracts.Orders;

public interface IOrdersModuleApi
{
    Task<Results.SubmitOrderResult> SubmitOrderAsync(Commands.SubmitOrderCommand command, Guid appUserId = default);

    Task<IEnumerable<OrderDto>> GetOrdersAsync(Guid appUserId = default);
    Task<OrderDto?> GetOrderAsync(Guid id, Guid appUserId = default);

    Task<OrderDto> CreateOrderAsync(OrderDto entity, Guid appUserId = default);
    Task<OrderDto> UpdateOrderDetailsAsync(OrderDto entity, Guid appUserId = default);
    Task DeleteOrderAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<OrderRowDto>> GetOrderRowsAsync(Guid appUserId = default);
    Task<OrderRowDto?> FindOrderRowAsync(Guid id, Guid appUserId = default);
    Task<OrderRowDto> CreateOrderRowAsync(OrderRowDto entity, Guid appUserId = default);
    Task<OrderRowDto> UpdateOrderRowDetailsAsync(OrderRowDto entity, Guid appUserId = default);
    Task DeleteOrderRowAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<PaymentDto>> GetPaymentsAsync(Guid appUserId = default);
    Task<PaymentDto?> FindPaymentAsync(Guid id, Guid appUserId = default);
    Task<PaymentDto> CreatePaymentAsync(PaymentDto entity, Guid appUserId = default);
    Task<PaymentDto> UpdatePaymentDetailsAsync(PaymentDto entity, Guid appUserId = default);
    Task DeletePaymentAsync(Guid id, Guid appUserId = default);
}
