namespace Base.Contracts;

/// <summary>
/// declares that this entity belongs to some app user
/// i.e. there is relationship defined via this FK value
/// this allows automatic universal filtering in dbcontext (IDOR)
/// </summary>

public interface IAppUserId : IAppUserId<Guid>
{
}

public interface IAppUserId<TKey>
    where TKey : IEquatable<TKey>
{
    public TKey UserId { get; set; }
}