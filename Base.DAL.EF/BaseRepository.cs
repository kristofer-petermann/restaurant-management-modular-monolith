using Base.Contracts;
using Base.DAL.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public BaseRepository(DbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> AllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<TEntity?> FindAsync(Guid id)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual TEntity Update(TEntity entity)
    {
        DbSet.Update(entity);
        return entity;
    }

    public virtual async Task RemoveAsync(Guid id)
    {
        var entity = await FindAsync(id);
        if (entity != null)
        {
            DbSet.Remove(entity);
        }
    }
}
