using MediatR;

namespace Shared.Contracts.Identity;

public sealed record HasRestaurantMembershipQuery(Guid UserId, Guid RestaurantId) : IRequest<bool>;
