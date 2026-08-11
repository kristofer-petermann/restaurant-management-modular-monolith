using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteOpeningHourCommand(Guid OpeningHourId, Guid AppUserId = default) : IRequest<bool>;
