using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class MenuCategoryService : Modules.Restaurants.Application.Services.IMenuCategoryService
{
    private readonly IMenuCategoryRepository _repository;

    public MenuCategoryService(IMenuCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MenuCategoryDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<MenuCategoryReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<MenuCategoryDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<MenuCategoryReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(MenuCategoryDto entity, Guid appUserId = default)
    {
        _repository.Add(ToDomain(entity));
    }

    public async Task<MenuCategoryDto> UpdateAsync(MenuCategoryDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Menu category {entity.Id} was not found.");
        dbEntity.MenuId = entity.MenuId;
        dbEntity.Name = UpsertTranslation(dbEntity.Name, entity.Name);
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        return ToDto(dbEntity);
    }

    public void Remove(MenuCategoryDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static MenuCategoryDto ToDto(Modules.Restaurants.Domain.MenuCategory entity)
    {
        return new MenuCategoryDto
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name,
            Summary = entity.Summary,
            Menu = entity.Menu == null
                ? null
                : new MenuCategoryMenuDto
                {
                    Id = entity.Menu.Id,
                    Name = entity.Menu.Name
                }
        };
    }

    private static Modules.Restaurants.Domain.MenuCategory ToDomain(MenuCategoryDto entity)
    {
        return new Modules.Restaurants.Domain.MenuCategory
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name ?? string.Empty,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static MenuCategoryReadDto ToReadDto(Modules.Restaurants.Domain.MenuCategory entity)
    {
        return new MenuCategoryReadDto
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name,
            Summary = entity.Summary,
            MenuName = entity.Menu?.Name
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
