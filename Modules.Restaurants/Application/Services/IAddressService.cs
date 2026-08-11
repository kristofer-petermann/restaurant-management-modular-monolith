using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IAddressService
{
    Task<IEnumerable<AddressReadDto>> AllReadAsync(Guid appUserId = default);
    Task<AddressReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<AddressDto>> AllAsync(Guid appUserId = default);
    Task<AddressDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(AddressDto entity, Guid appUserId = default);
    Task<AddressDto> UpdateAsync(AddressDto entity, Guid appUserId = default);
    void Remove(AddressDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);

    Task<List<Guid>> GetAllowedRestaurantIdsAsync(Guid appUserId);
    Task<List<RestaurantDto>> GetAvailableRestaurantsForAddressAsync(Guid? appUserId = null);
    Task<bool> ExistsAsync(Guid id);
}
