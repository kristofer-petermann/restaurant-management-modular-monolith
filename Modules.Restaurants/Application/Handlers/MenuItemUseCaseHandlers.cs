using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    private readonly IMenuItemService _menuItems;
    private readonly IRestaurantsUow _uow;

    public CreateMenuItemCommandHandler(IMenuItemService menuItems, IRestaurantsUow uow)
    {
        _menuItems = menuItems;
        _uow = uow;
    }

    public async Task<MenuItemDto> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new MenuItemDto
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = request.MenuCategoryId,
            Name = request.Name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? string.Empty : request.Summary
        };

        _menuItems.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, MenuItemDto?>
{
    private readonly IMenuItemService _menuItems;
    private readonly IRestaurantsUow _uow;

    public UpdateMenuItemCommandHandler(IMenuItemService menuItems, IRestaurantsUow uow)
    {
        _menuItems = menuItems;
        _uow = uow;
    }

    public async Task<MenuItemDto?> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menuItems.FindAsync(request.MenuItemId, request.AppUserId);
        if (entity == null) return null;

        entity.MenuCategoryId = request.MenuCategoryId;
        entity.Name = request.Name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description;
        entity.Price = request.Price;
        entity.IsAvailable = request.IsAvailable;
        entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl;
        entity.Summary = request.Summary;

        var updated = await _menuItems.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, bool>
{
    private readonly IMenuItemService _menuItems;
    private readonly IRestaurantsUow _uow;

    public DeleteMenuItemCommandHandler(IMenuItemService menuItems, IRestaurantsUow uow)
    {
        _menuItems = menuItems;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menuItems.FindAsync(request.MenuItemId, request.AppUserId);
        if (entity == null) return false;

        await _menuItems.RemoveAsync(request.MenuItemId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
