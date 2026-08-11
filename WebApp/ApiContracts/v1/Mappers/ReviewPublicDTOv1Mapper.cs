using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class ReviewPublicDTOv1Mapper : IBaseMapper<ReviewResponse, ReviewDto>
{
    public ReviewResponse? Map(ReviewReadDto? entity)
    {
        if (entity == null) return null;
        return new ReviewResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary
        };
    }

    public ReviewResponse? Map(ReviewDto? entity)
    {
        if (entity == null) return null;
        return new ReviewResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary
        };
    }

    public ReviewDto? Map(ReviewResponse? entity)
    {
        if (entity == null) return null;
        return new ReviewDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            Summary = entity.Summary
        };
    }
}
