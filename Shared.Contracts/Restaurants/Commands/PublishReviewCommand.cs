namespace Shared.Contracts.Restaurants.Commands;

public sealed record PublishReviewCommand(
    Guid RestaurantId,
    int Rating,
    string? Comment,
    string? Summary);
