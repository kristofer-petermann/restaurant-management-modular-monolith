using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Events;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Handlers;

internal sealed class GetRestaurantUsersQueryHandler : IRequestHandler<GetRestaurantUsersQuery, IReadOnlyList<RestaurantUserReadDto>>
{
    private readonly IRestaurantUserService _restaurantUsers;

    public GetRestaurantUsersQueryHandler(IRestaurantUserService restaurantUsers)
    {
        _restaurantUsers = restaurantUsers;
    }

    public async Task<IReadOnlyList<RestaurantUserReadDto>> Handle(GetRestaurantUsersQuery request, CancellationToken cancellationToken)
    {
        return (await _restaurantUsers.AllAsync(request.AppUserId)).ToList();
    }
}

internal sealed class GetRestaurantUserByIdQueryHandler : IRequestHandler<GetRestaurantUserByIdQuery, RestaurantUserReadDto?>
{
    private readonly IRestaurantUserService _restaurantUsers;

    public GetRestaurantUserByIdQueryHandler(IRestaurantUserService restaurantUsers)
    {
        _restaurantUsers = restaurantUsers;
    }

    public Task<RestaurantUserReadDto?> Handle(GetRestaurantUserByIdQuery request, CancellationToken cancellationToken)
    {
        return _restaurantUsers.FindAsync(request.Id, request.AppUserId);
    }
}

internal sealed class AssignRestaurantUserCommandHandler : IRequestHandler<AssignRestaurantUserCommand, RestaurantUserReadDto>
{
    private readonly IRestaurantUserService _restaurantUsers;
    private readonly IdentityDbContext _dbContext;
    private readonly IPublisher _publisher;

    public AssignRestaurantUserCommandHandler(
        IRestaurantUserService restaurantUsers,
        IdentityDbContext dbContext,
        IPublisher publisher)
    {
        _restaurantUsers = restaurantUsers;
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<RestaurantUserReadDto> Handle(AssignRestaurantUserCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        _restaurantUsers.Add(new RestaurantUserDto
        {
            Id = id,
            RestaurantId = request.RestaurantId,
            UserId = request.UserId,
            UserRole = request.UserRole,
            Role = request.Role,
            Summary = request.Summary
        }, request.AppUserId);

        await SaveChangesAsync(cancellationToken);
        return (await _restaurantUsers.FindAsync(id))!;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var newAssignments = _dbContext.ChangeTracker
            .Entries<RestaurantUser>()
            .Where(x => x.State == EntityState.Added)
            .Select(x => new RestaurantUserAssignedEvent(
                x.Entity.RestaurantId,
                x.Entity.UserId,
                x.Entity.UserRole.ToString(),
                DateTime.UtcNow))
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var assignment in newAssignments)
        {
            await _publisher.Publish(assignment, cancellationToken);
        }
    }
}

internal sealed class UpdateRestaurantUserCommandHandler : IRequestHandler<UpdateRestaurantUserCommand, RestaurantUserReadDto?>
{
    private readonly IRestaurantUserService _restaurantUsers;
    private readonly IdentityDbContext _dbContext;

    public UpdateRestaurantUserCommandHandler(
        IRestaurantUserService restaurantUsers,
        IdentityDbContext dbContext)
    {
        _restaurantUsers = restaurantUsers;
        _dbContext = dbContext;
    }

    public async Task<RestaurantUserReadDto?> Handle(UpdateRestaurantUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _restaurantUsers.FindEntityAsync(request.Id, request.AppUserId);
        if (existing == null) return null;

        existing.RestaurantId = request.RestaurantId;
        existing.UserId = request.UserId;
        existing.UserRole = request.UserRole;
        existing.Role = request.Role;
        existing.Summary = request.Summary;

        await _restaurantUsers.UpdateAsync(existing, request.AppUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _restaurantUsers.FindAsync(request.Id);
    }
}

internal sealed class RemoveRestaurantUserCommandHandler : IRequestHandler<RemoveRestaurantUserCommand, bool>
{
    private readonly IRestaurantUserService _restaurantUsers;
    private readonly IdentityDbContext _dbContext;

    public RemoveRestaurantUserCommandHandler(
        IRestaurantUserService restaurantUsers,
        IdentityDbContext dbContext)
    {
        _restaurantUsers = restaurantUsers;
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(RemoveRestaurantUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _restaurantUsers.FindEntityAsync(request.Id, request.AppUserId);
        if (existing == null) return false;

        await _restaurantUsers.RemoveAsync(request.Id, request.AppUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
