using MediatR;
using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Identity;

namespace Modules.Restaurants.Application.Services;

internal class AddressService : Modules.Restaurants.Application.Services.IAddressService
{
    private readonly IAddressRepository _repository;
    private readonly IMediator _mediator;

    public AddressService(IAddressRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<IEnumerable<AddressDto>> AllAsync(Guid appUserId = default)
    {
        var restaurantIds = appUserId != default ? await GetAllowedRestaurantIdsOrNullAsync(appUserId) : null;
        var entities = await _repository.GetAllAsync(appUserId, restaurantIds);
        var userDisplayMap = await ResolveUserDisplayMapAsync(entities.Select(e => e.UserId));
        return entities.Select(e => ToDto(e, userDisplayMap)).ToList();
    }

    public async Task<IEnumerable<AddressReadDto>> AllReadAsync(Guid appUserId = default)
    {
        var restaurantIds = appUserId != default ? await GetAllowedRestaurantIdsOrNullAsync(appUserId) : null;
        return (await _repository.GetAllAsync(appUserId, restaurantIds)).Select(ToReadDto).ToList();
    }

    public async Task<AddressDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var restaurantIds = appUserId != default ? await GetAllowedRestaurantIdsOrNullAsync(appUserId) : null;
        var entity = await _repository.FindByIdAsync(id, appUserId, restaurantIds);
        if (entity == null) return null;

        var userDisplayMap = await ResolveUserDisplayMapAsync(new[] { entity.UserId });
        return ToDto(entity, userDisplayMap);
    }

    public async Task<AddressReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var restaurantIds = appUserId != default ? await GetAllowedRestaurantIdsOrNullAsync(appUserId) : null;
        var entity = await _repository.FindByIdAsync(id, appUserId, restaurantIds);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(AddressDto entity, Guid appUserId = default)
    {
        var domain = ToDomain(entity);
        domain.UserId = appUserId == default ? domain.UserId : appUserId;
        EnsureAppUserExists(domain.UserId);
        _repository.Add(domain);
    }

    public async Task<AddressDto> UpdateAsync(AddressDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Address {entity.Id} was not found.");
        dbEntity.Street = entity.Street;
        dbEntity.City = entity.City;
        dbEntity.PostalCode = entity.PostalCode;
        dbEntity.Country = UpsertTranslation(dbEntity.Country, entity.Country);
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        dbEntity.UserId = appUserId == default ? entity.UserId : appUserId;
        dbEntity.RestaurantId = entity.RestaurantId;

        await EnsureAppUserExistsAsync(dbEntity.UserId);
        return ToDto(dbEntity, new Dictionary<Guid, string?>());
    }

    public void Remove(AddressDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id);
        if (entity != null) _repository.Remove(entity);
    }

    public async Task<List<Guid>> GetAllowedRestaurantIdsAsync(Guid appUserId)
    {
        return (await _mediator.Send(new GetAdminRestaurantIdsQuery(appUserId))).ToList();
    }

    public async Task<List<RestaurantDto>> GetAvailableRestaurantsForAddressAsync(Guid? appUserId = null)
    {
        IEnumerable<Guid>? allowedRestaurantIds = null;
        if (appUserId.HasValue)
        {
            allowedRestaurantIds = await GetAllowedRestaurantIdsAsync(appUserId.Value);
        }

        var restaurants = await _repository.GetRestaurantsAsync(allowedRestaurantIds);
        return restaurants.Select(r => new RestaurantDto
            {
                Id = r.Id,
                RestaurantName = r.RestaurantName
            })
            .ToList();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    private async Task<List<Guid>?> GetAllowedRestaurantIdsOrNullAsync(Guid appUserId)
    {
        if (appUserId == default) return null;
        var restaurantIds = await GetAllowedRestaurantIdsAsync(appUserId);
        return restaurantIds.Count > 0 ? restaurantIds : null;
    }

    private static AddressDto ToDto(Modules.Restaurants.Domain.Address entity, IReadOnlyDictionary<Guid, string?> userDisplayMap)
    {
        return new AddressDto
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            Summary = entity.Summary,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            User = entity.User == null
                ? null
                : new AddressUserDto
                {
                    Id = entity.User.Id,
                    Email = entity.User.Email ?? (entity.UserId.HasValue && userDisplayMap.TryGetValue(entity.UserId.Value, out var displayName) ? displayName : null)
                },
            Restaurant = entity.Restaurant == null
                ? null
                : new AddressRestaurantDto
                {
                    Id = entity.Restaurant.Id,
                    RestaurantName = entity.Restaurant.RestaurantName
                }
        };
    }

    private static AddressReadDto ToReadDto(Modules.Restaurants.Domain.Address entity)
    {
        return new AddressReadDto
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            Summary = entity.Summary,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            RestaurantName = entity.Restaurant?.RestaurantName,
            UserEmail = entity.User?.Email
        };
    }

    private static Modules.Restaurants.Domain.Address ToDomain(AddressDto entity)
    {
        return new Modules.Restaurants.Domain.Address
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country ?? string.Empty,
            Summary = entity.Summary ?? string.Empty,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId
        };
    }

    private async Task EnsureAppUserExistsAsync(Guid? userId)
    {
        if (!userId.HasValue) return;
        if (await _repository.AppUserExistsAsync(userId.Value)) return;

        _repository.AddAppUser(new RestaurantUserProfile
        {
            Id = userId.Value,
            UserName = userId.Value.ToString()
        });
    }

    private void EnsureAppUserExists(Guid? userId)
    {
        if (!userId.HasValue) return;
        if (_repository.AppUserExists(userId.Value)) return;

        _repository.AddAppUser(new RestaurantUserProfile
        {
            Id = userId.Value,
            UserName = userId.Value.ToString()
        });
    }

    private async Task<Dictionary<Guid, string?>> ResolveUserDisplayMapAsync(IEnumerable<Guid?> userIds)
    {
        var map = new Dictionary<Guid, string?>();
        foreach (var userId in userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct())
        {
            var response = await _mediator.Send(new GetUserDisplayNameQuery(userId));
            map[userId] = response?.DisplayName;
        }

        return map;
    }

    private static LangStr UpsertTranslation(LangStr? existing, string? value)
    {
        var val = value ?? string.Empty;
        existing ??= new LangStr(val);
        existing.SetTranslation(val);
        return existing;
    }
}
