using MediatR;

namespace Shared.Contracts.Identity;

public sealed record GetRestaurantUsersQuery(Guid AppUserId = default) : IRequest<IReadOnlyList<RestaurantUserReadDto>>;
