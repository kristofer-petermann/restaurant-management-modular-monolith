using Base.BLL.Contracts;
using Base.Contracts;
using Base.DAL.Contracts;

namespace Base.BLL;

public class BaseEntityService<TEntity> : IBaseEntityService<TEntity>
    where TEntity : class, IBaseEntity
{
    private readonly IBaseRepository<TEntity> _repository;
    private readonly IBaseUow _uow;

    public BaseEntityService(IBaseRepository<TEntity> repository, IBaseUow uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<IEnumerable<TEntity>> AllAsync()
    {
        return await _repository.AllAsync();
    }

    public async Task<TEntity?> FindAsync(Guid id)
    {
        return await _repository.FindAsync(id);
    }

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        _repository.Add(entity);
        await _uow.SaveChangesAsync();
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var updated = _repository.Update(entity);
        await _uow.SaveChangesAsync();
        return updated;
    }

    public async Task RemoveAsync(Guid id)
    {
        await _repository.RemoveAsync(id);
        await _uow.SaveChangesAsync();
    }
}
