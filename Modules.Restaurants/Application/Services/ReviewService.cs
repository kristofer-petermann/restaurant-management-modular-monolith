using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class ReviewService : Modules.Restaurants.Application.Services.IReviewService
{
    private readonly IReviewRepository _repository;
    public ReviewService(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReviewDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<ReviewReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<ReviewDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<ReviewReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(ReviewDto entity, Guid appUserId = default)
    {
        var domain = ToDomain(entity);
        domain.UserId = appUserId == default ? domain.UserId : appUserId;
        _repository.Add(domain);
    }

    public async Task<ReviewDto> UpdateAsync(ReviewDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Review {entity.Id} was not found.");
        dbEntity.RestaurantId = entity.RestaurantId;
        dbEntity.UserId = appUserId == default ? entity.UserId : appUserId;
        dbEntity.Rating = entity.Rating;
        dbEntity.Comment = entity.Comment ?? string.Empty;
        dbEntity.CreatedAt = entity.CreatedAt;
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        return ToDto(dbEntity);
    }

    public void Remove(ReviewDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static ReviewDto ToDto(Modules.Restaurants.Domain.Review entity)
    {
        return new ReviewDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary,
            AppUser = entity.AppUser == null
                ? null
                : new ReviewUserDto
                {
                    Id = entity.AppUser.Id,
                    Email = entity.AppUser.Email
                },
            Restaurant = entity.Restaurant == null
                ? null
                : new ReviewRestaurantDto
                {
                    Id = entity.Restaurant.Id,
                    RestaurantName = entity.Restaurant.RestaurantName
                }
        };
    }

    private static Modules.Restaurants.Domain.Review ToDomain(ReviewDto entity)
    {
        return new Modules.Restaurants.Domain.Review
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static ReviewReadDto ToReadDto(Modules.Restaurants.Domain.Review entity)
    {
        return new ReviewReadDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary,
            RestaurantName = entity.Restaurant?.RestaurantName,
            UserName = entity.AppUser?.UserName
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
