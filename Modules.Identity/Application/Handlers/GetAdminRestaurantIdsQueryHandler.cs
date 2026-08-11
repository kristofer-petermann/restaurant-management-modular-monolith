using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Handlers;

internal sealed class GetAdminRestaurantIdsQueryHandler : IRequestHandler<GetAdminRestaurantIdsQuery, IReadOnlyList<Guid>>
{
    private readonly IdentityDbContext _dbContext;

    public GetAdminRestaurantIdsQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Guid>> Handle(GetAdminRestaurantIdsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.RestaurantUsers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && x.UserRole == EUserRoleInRestaurant.Admin)
            .Select(x => x.RestaurantId)
            .ToListAsync(cancellationToken);
    }
}
