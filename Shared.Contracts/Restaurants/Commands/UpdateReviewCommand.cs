using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateReviewCommand(
    Guid ReviewId,
    Guid RestaurantId,
    Guid UserId,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    string? Summary,
    Guid AppUserId = default) : IRequest<ReviewDto?>;
