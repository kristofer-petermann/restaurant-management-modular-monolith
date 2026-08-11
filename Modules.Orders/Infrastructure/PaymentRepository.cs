using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;

namespace Modules.Orders.Infrastructure;

internal class PaymentRepository : IPaymentRepository
{
    private readonly OrdersDbContext _dbContext;

    public PaymentRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Payment>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payments
            .Include(x => x.Order)
            .AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Payment?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payments
            .Include(x => x.Order)
            .Where(x => x.Id == id)
            .AsQueryable();

        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Payment?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Payments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Payment?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payments.Where(x => x.Id == id).AsQueryable();
        if (appUserId != default)
        {
            query = query.Where(x => x.Order!.UserId == appUserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Payment entity)
    {
        _dbContext.Payments.Add(entity);
    }

    public void Remove(Payment entity)
    {
        _dbContext.Payments.Remove(entity);
    }
}
