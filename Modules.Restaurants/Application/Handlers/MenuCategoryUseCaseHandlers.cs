using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateMenuCategoryCommandHandler : IRequestHandler<CreateMenuCategoryCommand, MenuCategoryDto>
{
    private readonly IMenuCategoryService _menuCategories;
    private readonly IRestaurantsUow _uow;

    public CreateMenuCategoryCommandHandler(IMenuCategoryService menuCategories, IRestaurantsUow uow)
    {
        _menuCategories = menuCategories;
        _uow = uow;
    }

    public async Task<MenuCategoryDto> Handle(CreateMenuCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new MenuCategoryDto
        {
            Id = Guid.NewGuid(),
            MenuId = request.MenuId,
            Name = request.Name,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? string.Empty : request.Summary
        };

        _menuCategories.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateMenuCategoryCommandHandler : IRequestHandler<UpdateMenuCategoryCommand, MenuCategoryDto?>
{
    private readonly IMenuCategoryService _menuCategories;
    private readonly IRestaurantsUow _uow;

    public UpdateMenuCategoryCommandHandler(IMenuCategoryService menuCategories, IRestaurantsUow uow)
    {
        _menuCategories = menuCategories;
        _uow = uow;
    }

    public async Task<MenuCategoryDto?> Handle(UpdateMenuCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menuCategories.FindAsync(request.MenuCategoryId, request.AppUserId);
        if (entity == null) return null;

        entity.MenuId = request.MenuId;
        entity.Name = request.Name;
        entity.Summary = request.Summary;

        var updated = await _menuCategories.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteMenuCategoryCommandHandler : IRequestHandler<DeleteMenuCategoryCommand, bool>
{
    private readonly IMenuCategoryService _menuCategories;
    private readonly IRestaurantsUow _uow;

    public DeleteMenuCategoryCommandHandler(IMenuCategoryService menuCategories, IRestaurantsUow uow)
    {
        _menuCategories = menuCategories;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteMenuCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _menuCategories.FindAsync(request.MenuCategoryId, request.AppUserId);
        if (entity == null) return false;

        await _menuCategories.RemoveAsync(request.MenuCategoryId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
