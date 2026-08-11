using MediatR;

namespace Shared.Contracts.Identity;

public sealed record ValidateRestaurantUserPermissionQuery(Guid UserId, Guid RestaurantId, string Permission) : IRequest<bool>;
