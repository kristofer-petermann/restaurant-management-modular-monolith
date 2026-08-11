using MediatR;

namespace Shared.Contracts.Restaurants;

public sealed record ValidateMenuItemsQuery(IReadOnlyCollection<Guid> MenuItemIds) : IRequest<bool>;
