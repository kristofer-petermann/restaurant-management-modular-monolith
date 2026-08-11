using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Handlers;

internal sealed class GetUserDisplayNameQueryHandler : IRequestHandler<GetUserDisplayNameQuery, UserDisplayNameResponse?>
{
    private readonly IdentityDbContext _dbContext;

    public GetUserDisplayNameQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDisplayNameResponse?> Handle(GetUserDisplayNameQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user == null) return null;

        var displayName = user.UserName ?? user.Email ?? user.Id.ToString();
        return new UserDisplayNameResponse(user.Id, displayName);
    }
}
