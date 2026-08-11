using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using Modules.Restaurants.Domain;
using Shared.Contracts.Restaurants.Commands;
using Shared.Contracts.Restaurants.Results;

namespace Modules.Restaurants.Application;

internal class RestaurantsModuleApi : IRestaurantsModuleApi
{
    private readonly RestaurantsDbContext _dbContext;
    private readonly IRestaurantsUow _uow;
    private readonly IAddressService _addresses;
    private readonly IMenuCategoryService _menuCategories;
    private readonly IMenuItemService _menuItems;
    private readonly IMenuService _menus;
    private readonly IOpeningHourService _openingHours;
    private readonly IRestaurantService _restaurants;
    private readonly IReviewService _reviews;
    private readonly IPublisher _publisher;

    public RestaurantsModuleApi(
        Modules.Restaurants.Application.Services.IAddressService addresses,
        Modules.Restaurants.Application.Services.IMenuCategoryService menuCategories,
        Modules.Restaurants.Application.Services.IMenuItemService menuItems,
        Modules.Restaurants.Application.Services.IMenuService menus,
        Modules.Restaurants.Application.Services.IOpeningHourService openingHours,
        Modules.Restaurants.Application.Services.IRestaurantService restaurants,
        Modules.Restaurants.Application.Services.IReviewService reviews,
        IRestaurantsUow uow,
        RestaurantsDbContext dbContext,
        IPublisher publisher)
    {
        _addresses = addresses;
        _menuCategories = menuCategories;
        _menuItems = menuItems;
        _menus = menus;
        _openingHours = openingHours;
        _restaurants = restaurants;
        _reviews = reviews;
        _uow = uow;
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<PublishReviewResult> PublishReviewAsync(PublishReviewCommand command, Guid appUserId = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (appUserId == default) throw new ArgumentException("appUserId must be provided.", nameof(appUserId));
        if (command.Rating is < 1 or > 10) throw new ArgumentException("Rating must be between 1 and 10.", nameof(command));

        var restaurantExists = await _restaurants.FindAsync(command.RestaurantId) != null;
        if (!restaurantExists) throw new InvalidOperationException("Restaurant does not exist.");

        var reviewId = Guid.NewGuid();
        var review = new ReviewDto
        {
            Id = reviewId,
            UserId = appUserId,
            RestaurantId = command.RestaurantId,
            Rating = command.Rating,
            Comment = string.IsNullOrWhiteSpace(command.Comment) ? string.Empty : command.Comment,
            CreatedAt = DateTime.UtcNow,
            Summary = string.IsNullOrWhiteSpace(command.Summary) ? string.Empty : command.Summary
        };

        _reviews.Add(review, appUserId);
        await SaveChangesAsync();
        return new PublishReviewResult(reviewId);
    }

    public Task<IEnumerable<AddressReadDto>> GetAddressReadsAsync(Guid appUserId = default) => _addresses.AllReadAsync(appUserId);
    public Task<AddressReadDto?> FindAddressReadAsync(Guid id, Guid appUserId = default) => _addresses.FindReadAsync(id, appUserId);

    public Task<IEnumerable<AddressDto>> GetAddressesAsync(Guid appUserId = default) => _addresses.AllAsync(appUserId);
    public Task<AddressDto?> FindAddressAsync(Guid id, Guid appUserId = default) => _addresses.FindAsync(id, appUserId);
    public Task<bool> AddressExistsAsync(Guid id) => _addresses.ExistsAsync(id);
    public Task<List<Guid>> GetAllowedRestaurantIdsForAddressAsync(Guid appUserId) => _addresses.GetAllowedRestaurantIdsAsync(appUserId);
    public Task<List<RestaurantDto>> GetAvailableRestaurantsForAddressAsync(Guid? appUserId = null) => _addresses.GetAvailableRestaurantsForAddressAsync(appUserId);

    public Task<IEnumerable<MenuCategoryDto>> GetMenuCategoriesAsync(Guid appUserId = default) => _menuCategories.AllAsync(appUserId);
    public Task<IEnumerable<MenuCategoryReadDto>> GetMenuCategoryReadsAsync(Guid appUserId = default) => _menuCategories.AllReadAsync(appUserId);
    public Task<MenuCategoryDto?> FindMenuCategoryAsync(Guid id, Guid appUserId = default) => _menuCategories.FindAsync(id, appUserId);
    public Task<MenuCategoryReadDto?> FindMenuCategoryReadAsync(Guid id, Guid appUserId = default) => _menuCategories.FindReadAsync(id, appUserId);

    public Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(Guid appUserId = default) => _menuItems.AllAsync(appUserId);
    public Task<IEnumerable<MenuItemReadDto>> GetMenuItemReadsAsync(Guid appUserId = default) => _menuItems.AllReadAsync(appUserId);
    public Task<MenuItemDto?> FindMenuItemAsync(Guid id, Guid appUserId = default) => _menuItems.FindAsync(id, appUserId);
    public Task<MenuItemReadDto?> FindMenuItemReadAsync(Guid id, Guid appUserId = default) => _menuItems.FindReadAsync(id, appUserId);

    public Task<IEnumerable<MenuDto>> GetMenusAsync(Guid appUserId = default) => _menus.AllAsync(appUserId);
    public Task<IEnumerable<MenuReadDto>> GetMenuReadsAsync(Guid appUserId = default) => _menus.AllReadAsync(appUserId);
    public Task<MenuDto?> FindMenuAsync(Guid id, Guid appUserId = default) => _menus.FindAsync(id, appUserId);
    public Task<MenuReadDto?> FindMenuReadAsync(Guid id, Guid appUserId = default) => _menus.FindReadAsync(id, appUserId);

    public Task<IEnumerable<OpeningHourDto>> GetOpeningHoursAsync(Guid appUserId = default) => _openingHours.AllAsync(appUserId);
    public Task<IEnumerable<OpeningHourReadDto>> GetOpeningHourReadsAsync(Guid appUserId = default) => _openingHours.AllReadAsync(appUserId);
    public Task<OpeningHourDto?> FindOpeningHourAsync(Guid id, Guid appUserId = default) => _openingHours.FindAsync(id, appUserId);
    public Task<OpeningHourReadDto?> FindOpeningHourReadAsync(Guid id, Guid appUserId = default) => _openingHours.FindReadAsync(id, appUserId);

    public Task<IEnumerable<RestaurantDto>> GetRestaurantsAsync(Guid appUserId = default) => _restaurants.AllAsync(appUserId);
    public Task<IEnumerable<RestaurantReadDto>> GetRestaurantReadsAsync(Guid appUserId = default) => _restaurants.AllReadAsync(appUserId);
    public Task<RestaurantDto?> FindRestaurantAsync(Guid id, Guid appUserId = default) => _restaurants.FindAsync(id, appUserId);
    public Task<RestaurantReadDto?> FindRestaurantReadAsync(Guid id, Guid appUserId = default) => _restaurants.FindReadAsync(id, appUserId);

    public Task<IEnumerable<ReviewDto>> GetReviewsAsync(Guid appUserId = default) => _reviews.AllAsync(appUserId);
    public Task<IEnumerable<ReviewReadDto>> GetReviewReadsAsync(Guid appUserId = default) => _reviews.AllReadAsync(appUserId);
    public Task<ReviewDto?> FindReviewAsync(Guid id, Guid appUserId = default) => _reviews.FindAsync(id, appUserId);
    public Task<ReviewReadDto?> FindReviewReadAsync(Guid id, Guid appUserId = default) => _reviews.FindReadAsync(id, appUserId);
    public async Task<ReviewDto> CreateReviewAsync(ReviewDto entity, Guid appUserId = default)
    {
        _reviews.Add(entity, appUserId);
        await SaveChangesAsync();
        return entity;
    }

    private async Task<int> SaveChangesAsync()
    {
        var createdReviews = _dbContext.ChangeTracker
            .Entries<Review>()
            .Where(x => x.State == EntityState.Added)
            .Select(x => new ReviewCreatedEvent(
                x.Entity.Id,
                x.Entity.RestaurantId,
                x.Entity.UserId,
                x.Entity.Rating,
                x.Entity.CreatedAt))
            .ToList();

        var result = await _uow.SaveChangesAsync();

        foreach (var createdReview in createdReviews)
        {
            await _publisher.Publish(createdReview);
        }

        return result;
    }
}
