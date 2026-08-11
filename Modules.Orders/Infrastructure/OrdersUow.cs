namespace Modules.Orders.Infrastructure;

internal class OrdersUow : IOrdersUow
{
    private readonly OrdersDbContext _dbContext;

    public OrdersUow(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
