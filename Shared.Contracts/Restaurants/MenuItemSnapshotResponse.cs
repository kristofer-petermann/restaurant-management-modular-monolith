namespace Shared.Contracts.Restaurants;

public sealed record MenuItemSnapshotResponse(Guid MenuItemId, string Name, decimal Price, bool Exists);
