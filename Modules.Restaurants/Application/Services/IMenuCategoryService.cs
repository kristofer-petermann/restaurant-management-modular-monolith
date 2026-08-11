using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IMenuCategoryService
{
    Task<IEnumerable<MenuCategoryReadDto>> AllReadAsync(Guid appUserId = default);
    Task<MenuCategoryReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<MenuCategoryDto>> AllAsync(Guid appUserId = default);
    Task<MenuCategoryDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(MenuCategoryDto entity, Guid appUserId = default);
    Task<MenuCategoryDto> UpdateAsync(MenuCategoryDto entity, Guid appUserId = default);
    void Remove(MenuCategoryDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
