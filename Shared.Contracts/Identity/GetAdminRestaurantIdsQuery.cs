using MediatR;

namespace Shared.Contracts.Identity;

public sealed record GetAdminRestaurantIdsQuery(Guid UserId) : IRequest<IReadOnlyList<Guid>>;
