using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteMenuCategoryCommand(Guid MenuCategoryId, Guid AppUserId = default) : IRequest<bool>;
