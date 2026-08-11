using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateMenuCategoryCommand(
    Guid MenuCategoryId,
    Guid MenuId,
    string? Name,
    string? Summary,
    Guid AppUserId = default) : IRequest<MenuCategoryDto?>;
