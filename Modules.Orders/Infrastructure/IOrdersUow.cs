namespace Modules.Orders.Infrastructure;

internal interface IOrdersUow
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
