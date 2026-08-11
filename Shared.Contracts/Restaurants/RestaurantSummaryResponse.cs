namespace Shared.Contracts.Restaurants;

public sealed record RestaurantSummaryResponse(Guid RestaurantId, string RestaurantName, string? Summary);
