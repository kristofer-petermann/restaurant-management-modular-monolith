using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IMenuService
{
    Task<IEnumerable<MenuReadDto>> AllReadAsync(Guid appUserId = default);
    Task<MenuReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<MenuDto>> AllAsync(Guid appUserId = default);
    Task<MenuDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(MenuDto entity, Guid appUserId = default);
    Task<MenuDto> UpdateAsync(MenuDto entity, Guid appUserId = default);
    void Remove(MenuDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
