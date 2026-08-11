namespace Base.DAL.Contracts;

public interface IBaseUow
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
