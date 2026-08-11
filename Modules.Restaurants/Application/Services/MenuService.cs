using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class MenuService : Modules.Restaurants.Application.Services.IMenuService
{
    private readonly IMenuRepository _repository;

    public MenuService(IMenuRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<MenuReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<MenuDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<MenuReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(MenuDto entity, Guid appUserId = default)
    {
        _repository.Add(ToDomain(entity));
    }

    public async Task<MenuDto> UpdateAsync(MenuDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Menu {entity.Id} was not found.");
        dbEntity.RestaurantId = entity.RestaurantId;
        dbEntity.Name = UpsertTranslation(dbEntity.Name, entity.Name);
        dbEntity.IsActive = entity.IsActive;
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        return ToDto(dbEntity);
    }

    public void Remove(MenuDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static MenuDto ToDto(Modules.Restaurants.Domain.Menu entity)
    {
        return new MenuDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            Summary = entity.Summary,
            Restaurant = entity.Restaurant == null
                ? null
                : new MenuRestaurantDto
                {
                    Id = entity.Restaurant.Id,
                    RestaurantName = entity.Restaurant.RestaurantName
                }
        };
    }

    private static Modules.Restaurants.Domain.Menu ToDomain(MenuDto entity)
    {
        return new Modules.Restaurants.Domain.Menu
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name ?? string.Empty,
            IsActive = entity.IsActive,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static MenuReadDto ToReadDto(Modules.Restaurants.Domain.Menu entity)
    {
        return new MenuReadDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            Summary = entity.Summary,
            RestaurantName = entity.Restaurant?.RestaurantName
        };
    }

    private static LangStr UpsertTranslation(LangStr? existing, string? value)
    {
        var val = value ?? string.Empty;
        existing ??= new LangStr(val);
        existing.SetTranslation(val);
        return existing;
    }
}
