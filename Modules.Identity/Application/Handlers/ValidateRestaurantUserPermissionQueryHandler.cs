using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Handlers;

internal sealed class ValidateRestaurantUserPermissionQueryHandler : IRequestHandler<ValidateRestaurantUserPermissionQuery, bool>
{
    private readonly IdentityDbContext _dbContext;

    public ValidateRestaurantUserPermissionQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(ValidateRestaurantUserPermissionQuery request, CancellationToken cancellationToken)
    {
        var normalizedPermission = request.Permission.Trim().ToLowerInvariant();

        return await _dbContext.RestaurantUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == request.UserId &&
                x.RestaurantId == request.RestaurantId &&
                (x.UserRole == EUserRoleInRestaurant.Admin ||
                 (x.Role != null && x.Role.ToLower() == normalizedPermission)),
                cancellationToken);
    }
}
