namespace Base.Contracts;

public interface IBaseEntity : IBaseEntity<Guid>
{
}

public interface IBaseEntity<TKey>
    where TKey : IEquatable<TKey>
{
    public Guid Id { get; set; }
}