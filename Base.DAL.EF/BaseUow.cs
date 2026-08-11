using Base.DAL.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class BaseUow : IBaseUow
{
    protected readonly DbContext DbContext;

    public BaseUow(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
