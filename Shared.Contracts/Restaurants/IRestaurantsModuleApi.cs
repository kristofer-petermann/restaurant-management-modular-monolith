namespace Shared.Contracts.Restaurants;

public interface IRestaurantsModuleApi
{
    Task<Results.PublishReviewResult> PublishReviewAsync(Commands.PublishReviewCommand command, Guid appUserId = default);

    Task<IEnumerable<AddressReadDto>> GetAddressReadsAsync(Guid appUserId = default);
    Task<AddressReadDto?> FindAddressReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<AddressDto>> GetAddressesAsync(Guid appUserId = default);
    Task<AddressDto?> FindAddressAsync(Guid id, Guid appUserId = default);
    Task<bool> AddressExistsAsync(Guid id);
    Task<List<Guid>> GetAllowedRestaurantIdsForAddressAsync(Guid appUserId);
    Task<List<RestaurantDto>> GetAvailableRestaurantsForAddressAsync(Guid? appUserId = null);

    Task<IEnumerable<MenuCategoryDto>> GetMenuCategoriesAsync(Guid appUserId = default);
    Task<IEnumerable<MenuCategoryReadDto>> GetMenuCategoryReadsAsync(Guid appUserId = default);
    Task<MenuCategoryDto?> FindMenuCategoryAsync(Guid id, Guid appUserId = default);
    Task<MenuCategoryReadDto?> FindMenuCategoryReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(Guid appUserId = default);
    Task<IEnumerable<MenuItemReadDto>> GetMenuItemReadsAsync(Guid appUserId = default);
    Task<MenuItemDto?> FindMenuItemAsync(Guid id, Guid appUserId = default);
    Task<MenuItemReadDto?> FindMenuItemReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<MenuDto>> GetMenusAsync(Guid appUserId = default);
    Task<IEnumerable<MenuReadDto>> GetMenuReadsAsync(Guid appUserId = default);
    Task<MenuDto?> FindMenuAsync(Guid id, Guid appUserId = default);
    Task<MenuReadDto?> FindMenuReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<OpeningHourDto>> GetOpeningHoursAsync(Guid appUserId = default);
    Task<IEnumerable<OpeningHourReadDto>> GetOpeningHourReadsAsync(Guid appUserId = default);
    Task<OpeningHourDto?> FindOpeningHourAsync(Guid id, Guid appUserId = default);
    Task<OpeningHourReadDto?> FindOpeningHourReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<RestaurantDto>> GetRestaurantsAsync(Guid appUserId = default);
    Task<IEnumerable<RestaurantReadDto>> GetRestaurantReadsAsync(Guid appUserId = default);
    Task<RestaurantDto?> FindRestaurantAsync(Guid id, Guid appUserId = default);
    Task<RestaurantReadDto?> FindRestaurantReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<ReviewDto>> GetReviewsAsync(Guid appUserId = default);
    Task<IEnumerable<ReviewReadDto>> GetReviewReadsAsync(Guid appUserId = default);
    Task<ReviewDto?> FindReviewAsync(Guid id, Guid appUserId = default);
    Task<ReviewReadDto?> FindReviewReadAsync(Guid id, Guid appUserId = default);
    Task<ReviewDto> CreateReviewAsync(ReviewDto entity, Guid appUserId = default);
}
