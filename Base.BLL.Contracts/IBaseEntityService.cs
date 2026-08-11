using Base.Contracts;

namespace Base.BLL.Contracts;

public interface IBaseEntityService<TEntity>
    where TEntity : class, IBaseEntity
{
    Task<IEnumerable<TEntity>> AllAsync();
    Task<TEntity?> FindAsync(Guid id);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task RemoveAsync(Guid id);
}
