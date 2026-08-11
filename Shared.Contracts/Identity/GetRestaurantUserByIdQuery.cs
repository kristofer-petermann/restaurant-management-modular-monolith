using MediatR;

namespace Shared.Contracts.Identity;

public sealed record GetRestaurantUserByIdQuery(Guid Id, Guid AppUserId = default) : IRequest<RestaurantUserReadDto?>;
