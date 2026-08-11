using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Order>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.UserId == appUserId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Order?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.Where(x => x.Id == id).AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Order?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Order?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(x => x.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Order entity)
    {
        _dbContext.Orders.Add(entity);
    }

    public void Remove(Order entity)
    {
        _dbContext.Orders.Remove(entity);
    }
}
