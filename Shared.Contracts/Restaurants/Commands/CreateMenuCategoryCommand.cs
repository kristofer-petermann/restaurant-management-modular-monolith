using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record CreateMenuCategoryCommand(
    Guid MenuId,
    string? Name,
    string? Summary,
    Guid AppUserId = default) : IRequest<MenuCategoryDto>;
