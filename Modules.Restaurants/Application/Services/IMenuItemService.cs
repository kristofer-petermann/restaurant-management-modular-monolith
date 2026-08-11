using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IMenuItemService
{
    Task<IEnumerable<MenuItemReadDto>> AllReadAsync(Guid appUserId = default);
    Task<MenuItemReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<MenuItemDto>> AllAsync(Guid appUserId = default);
    Task<MenuItemDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(MenuItemDto entity, Guid appUserId = default);
    Task<MenuItemDto> UpdateAsync(MenuItemDto entity, Guid appUserId = default);
    void Remove(MenuItemDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
