using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, MenuDto>
{
    private readonly IMenuService _menus;
    private readonly IRestaurantsUow _uow;

    public CreateMenuCommandHandler(IMenuService menus, IRestaurantsUow uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task<MenuDto> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var entity = new MenuDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = request.RestaurantId,
            Name = request.Name,
            IsActive = request.IsActive,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? string.Empty : request.Summary
        };

        _menus.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, MenuDto?>
{
    private readonly IMenuService _menus;
    private readonly IRestaurantsUow _uow;

    public UpdateMenuCommandHandler(IMenuService menus, IRestaurantsUow uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task<MenuDto?> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menus.FindAsync(request.MenuId, request.AppUserId);
        if (entity == null) return null;

        entity.RestaurantId = request.RestaurantId;
        entity.Name = request.Name;
        entity.IsActive = request.IsActive;
        entity.Summary = request.Summary;

        var updated = await _menus.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
{
    private readonly IMenuService _menus;
    private readonly IRestaurantsUow _uow;

    public DeleteMenuCommandHandler(IMenuService menus, IRestaurantsUow uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menus.FindAsync(request.MenuId, request.AppUserId);
        if (entity == null) return false;

        await _menus.RemoveAsync(request.MenuId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
