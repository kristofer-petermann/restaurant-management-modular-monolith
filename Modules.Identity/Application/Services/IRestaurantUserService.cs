using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Services;

internal interface IRestaurantUserService
{
    Task<IEnumerable<RestaurantUserReadDto>> AllAsync(Guid appUserId = default);
    Task<RestaurantUserReadDto?> FindAsync(Guid id, Guid appUserId = default);
    Task<RestaurantUserDto?> FindEntityAsync(Guid id, Guid appUserId = default);
    void Add(RestaurantUserDto entity, Guid appUserId = default);
    Task<RestaurantUserDto> UpdateAsync(RestaurantUserDto entity, Guid appUserId = default);
    void Remove(RestaurantUserDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);

    Task<bool> IsRestaurantAdminAsync(Guid userId, Guid? restaurantId = null);
    Task<List<Guid>> GetAdminRestaurantIdsAsync(Guid userId);
    Task<List<AllowedRestaurantDto>> GetAllowedRestaurantsAsync(Guid userId);
    Task<List<UserOptionDto>> GetUsersAsync();
}
