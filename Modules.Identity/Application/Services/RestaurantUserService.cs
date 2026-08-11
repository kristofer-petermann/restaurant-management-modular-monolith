using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Identity;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Shared.Contracts.Restaurants;

namespace Modules.Identity.Application.Services;

internal class RestaurantUserService : Modules.Identity.Application.Services.IRestaurantUserService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IMediator _mediator;

    public RestaurantUserService(
        IdentityDbContext dbContext,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<IEnumerable<RestaurantUserReadDto>> AllAsync(Guid appUserId = default)
    {
        var query = _dbContext.RestaurantUsers.Include(ru => ru.User).AsQueryable();
        if (appUserId != default) query = query.Where(ru => ru.UserId == appUserId);

        var rows = await query.ToListAsync();
        var restaurantNames = await ResolveRestaurantNamesAsync(rows.Select(x => x.RestaurantId));
        return rows.Select(row => ToReadDto(row, restaurantNames)).ToList();
    }

    public async Task<RestaurantUserReadDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var query = _dbContext.RestaurantUsers.Include(ru => ru.User).Where(ru => ru.Id == id).AsQueryable();
        if (appUserId != default) query = query.Where(ru => ru.UserId == appUserId);

        var row = await query.FirstOrDefaultAsync();
        if (row == null) return null;

        var summary = await _mediator.Send(new GetRestaurantSummaryQuery(row.RestaurantId));
        return ToReadDto(row, summary == null
            ? new Dictionary<Guid, string?>()
            : new Dictionary<Guid, string?> { [row.RestaurantId] = summary.RestaurantName });
    }

    public async Task<RestaurantUserDto?> FindEntityAsync(Guid id, Guid appUserId = default)
    {
        var query = _dbContext.RestaurantUsers.Include(ru => ru.User).Where(ru => ru.Id == id).AsQueryable();
        if (appUserId != default) query = query.Where(ru => ru.UserId == appUserId);

        var row = await query.FirstOrDefaultAsync();
        return row == null ? null : ToDto(row);
    }

    public void Add(RestaurantUserDto entity, Guid appUserId = default)
    {
        var domain = ToDomain(entity);
        domain.UserId = appUserId == default ? domain.UserId : appUserId;
        _dbContext.RestaurantUsers.Add(domain);
    }

    public async Task<RestaurantUserDto> UpdateAsync(RestaurantUserDto entity, Guid appUserId = default)
    {
        var dbEntity = await _dbContext.RestaurantUsers.FirstAsync(x => x.Id == entity.Id);
        dbEntity.RestaurantId = entity.RestaurantId;
        dbEntity.UserId = appUserId == default ? entity.UserId : appUserId;
        dbEntity.UserRole = (EUserRoleInRestaurant)entity.UserRole;
        dbEntity.Role = entity.Role ?? string.Empty;
        dbEntity.Summary = entity.Summary ?? string.Empty;

        _dbContext.RestaurantUsers.Update(dbEntity);
        return ToDto(dbEntity);
    }

    public void Remove(RestaurantUserDto entity)
    {
        var domain = ToDomain(entity);
        _dbContext.RestaurantUsers.Remove(domain);
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _dbContext.RestaurantUsers.FirstOrDefaultAsync(x =>
            x.Id == id && (appUserId == default || x.UserId == appUserId));
        if (entity != null) _dbContext.RestaurantUsers.Remove(entity);
    }

    public async Task<bool> IsRestaurantAdminAsync(Guid userId, Guid? restaurantId = null)
    {
        var query = _dbContext.RestaurantUsers
            .Where(ru => ru.UserId == userId && ru.UserRole == EUserRoleInRestaurant.Admin);

        if (restaurantId.HasValue) query = query.Where(ru => ru.RestaurantId == restaurantId.Value);
        return await query.AnyAsync();
    }

    public async Task<List<Guid>> GetAdminRestaurantIdsAsync(Guid userId)
    {
        return await _dbContext.RestaurantUsers
            .Where(ru => ru.UserId == userId && ru.UserRole == EUserRoleInRestaurant.Admin)
            .Select(ru => ru.RestaurantId)
            .ToListAsync();
    }

    public async Task<List<AllowedRestaurantDto>> GetAllowedRestaurantsAsync(Guid userId)
    {
        var restaurantIds = await GetAdminRestaurantIdsAsync(userId);
        var result = new List<AllowedRestaurantDto>(restaurantIds.Count);
        foreach (var restaurantId in restaurantIds)
        {
            var summary = await _mediator.Send(new GetRestaurantSummaryQuery(restaurantId));
            if (summary == null) continue;

            result.Add(new AllowedRestaurantDto
            {
                Id = summary.RestaurantId,
                Name = summary.RestaurantName
            });
        }
        return result;
    }

    public async Task<List<UserOptionDto>> GetUsersAsync()
    {
        return (await _dbContext.Users
                .Select(u => new UserOptionDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .ToListAsync())
            .ToList();
    }

    private static RestaurantUserReadDto ToReadDto(RestaurantUser domain, IReadOnlyDictionary<Guid, string?> restaurantNames)
    {
        restaurantNames.TryGetValue(domain.RestaurantId, out var restaurantName);

        return new RestaurantUserReadDto
        {
            Id = domain.Id,
            RestaurantId = domain.RestaurantId,
            RestaurantName = restaurantName,
            UserId = domain.UserId,
            UserEmail = domain.User?.Email,
            UserRole = (RestaurantUserRole)domain.UserRole,
            Role = domain.Role,
            Summary = domain.Summary
        };
    }

    private static RestaurantUserDto ToDto(RestaurantUser domain)
    {
        return new RestaurantUserDto
        {
            Id = domain.Id,
            RestaurantId = domain.RestaurantId,
            UserId = domain.UserId,
            UserRole = (RestaurantUserRole)domain.UserRole,
            Role = domain.Role,
            Summary = domain.Summary
        };
    }

    private static RestaurantUser ToDomain(RestaurantUserDto entity)
    {
        return new RestaurantUser
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            UserId = entity.UserId,
            UserRole = (EUserRoleInRestaurant)entity.UserRole,
            Role = entity.Role ?? string.Empty,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private async Task<Dictionary<Guid, string?>> ResolveRestaurantNamesAsync(IEnumerable<Guid> restaurantIds)
    {
        var names = new Dictionary<Guid, string?>();
        foreach (var restaurantId in restaurantIds.Distinct())
        {
            var summary = await _mediator.Send(new GetRestaurantSummaryQuery(restaurantId));
            names[restaurantId] = summary?.RestaurantName;
        }

        return names;
    }
}
