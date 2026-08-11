using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Handlers;

internal sealed class HasRestaurantMembershipQueryHandler : IRequestHandler<HasRestaurantMembershipQuery, bool>
{
    private readonly IdentityDbContext _dbContext;

    public HasRestaurantMembershipQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> Handle(HasRestaurantMembershipQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.RestaurantUsers
            .AsNoTracking()
            .AnyAsync(x => x.UserId == request.UserId && x.RestaurantId == request.RestaurantId, cancellationToken);
    }
}
