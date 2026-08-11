using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteMenuCommand(Guid MenuId, Guid AppUserId = default) : IRequest<bool>;
