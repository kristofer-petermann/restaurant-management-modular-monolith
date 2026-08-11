using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateMenuItemCommand(
    Guid MenuItemId,
    Guid MenuCategoryId,
    string? Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl,
    string? Summary,
    Guid AppUserId = default) : IRequest<MenuItemDto?>;
