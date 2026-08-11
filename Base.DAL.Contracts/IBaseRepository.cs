using Base.Contracts;

namespace Base.DAL.Contracts;

public interface IBaseRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    Task<IEnumerable<TEntity>> AllAsync();
    Task<TEntity?> FindAsync(Guid id);
    void Add(TEntity entity);
    TEntity Update(TEntity entity);
    Task RemoveAsync(Guid id);
}
