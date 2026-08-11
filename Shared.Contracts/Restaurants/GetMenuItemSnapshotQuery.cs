using MediatR;

namespace Shared.Contracts.Restaurants;

public sealed record GetMenuItemSnapshotQuery(Guid MenuItemId) : IRequest<MenuItemSnapshotResponse?>;
