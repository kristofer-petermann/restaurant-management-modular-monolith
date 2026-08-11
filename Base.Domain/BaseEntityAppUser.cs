using Base.Contracts;

namespace Base.Domain;

public class BaseEntityAppUser : BaseEntity, IAppUserId
{
    public Guid UserId { get; set; }
}