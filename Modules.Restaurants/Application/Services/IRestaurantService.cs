using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IRestaurantService
{
    Task<IEnumerable<RestaurantReadDto>> AllReadAsync(Guid appUserId = default);
    Task<RestaurantReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<RestaurantDto>> AllAsync(Guid appUserId = default);
    Task<RestaurantDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(RestaurantDto entity, Guid appUserId = default);
    Task<RestaurantDto> UpdateAsync(RestaurantDto entity, Guid appUserId = default);
    void Remove(RestaurantDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
