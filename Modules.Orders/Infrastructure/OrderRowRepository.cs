using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal class OrderRowRepository : IOrderRowRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderRowRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrderRow>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OrderRows
            .Include(x => x.Order)
            .AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<OrderRow?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OrderRows
            .Include(x => x.Order)
            .Where(x => x.Id == id)
            .AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<OrderRow?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderRows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<OrderRow?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OrderRows.Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(OrderRow entity)
    {
        _dbContext.OrderRows.Add(entity);
    }

    public void Remove(OrderRow entity)
    {
        _dbContext.OrderRows.Remove(entity);
    }
}
