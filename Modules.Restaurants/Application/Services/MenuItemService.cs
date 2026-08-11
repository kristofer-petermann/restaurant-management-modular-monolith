using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class MenuItemService : Modules.Restaurants.Application.Services.IMenuItemService
{
    private readonly IMenuItemRepository _repository;

    public MenuItemService(IMenuItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuItemDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<MenuItemReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<MenuItemDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<MenuItemReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(MenuItemDto entity, Guid appUserId = default)
    {
        _repository.Add(ToDomain(entity));
    }

    public async Task<MenuItemDto> UpdateAsync(MenuItemDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Menu item {entity.Id} was not found.");
        dbEntity.MenuCategoryId = entity.MenuCategoryId;
        dbEntity.Name = UpsertTranslation(dbEntity.Name, entity.Name);
        dbEntity.Description = UpsertTranslation(dbEntity.Description, entity.Description);
        dbEntity.Price = entity.Price;
        dbEntity.IsAvailable = entity.IsAvailable;
        dbEntity.ImageUrl = entity.ImageUrl;
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        return ToDto(dbEntity);
    }

    public void Remove(MenuItemDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static MenuItemDto ToDto(Modules.Restaurants.Domain.MenuItem entity)
    {
        return new MenuItemDto
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary,
            MenuCategory = entity.MenuCategory == null
                ? null
                : new MenuItemCategoryDto
                {
                    Id = entity.MenuCategory.Id,
                    Name = entity.MenuCategory.Name
                }
        };
    }

    private static Modules.Restaurants.Domain.MenuItem ToDomain(MenuItemDto entity)
    {
        return new Modules.Restaurants.Domain.MenuItem
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description ?? string.Empty,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static MenuItemReadDto ToReadDto(Modules.Restaurants.Domain.MenuItem entity)
    {
        return new MenuItemReadDto
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary,
            MenuCategoryName = entity.MenuCategory?.Name
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
