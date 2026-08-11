using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class MenuItemPublicDTOv1Mapper : IBaseMapper<MenuItemResponse, MenuItemDto>
{
    public MenuItemResponse? Map(MenuItemReadDto? entity)
    {
        if (entity == null) return null;
        return new MenuItemResponse
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary
        };
    }

    public MenuItemResponse? Map(MenuItemDto? entity)
    {
        if (entity == null) return null;
        return new MenuItemResponse
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary
        };
    }

    public MenuItemDto? Map(MenuItemResponse? entity)
    {
        if (entity == null) return null;
        return new MenuItemDto
        {
            Id = entity.Id,
            MenuCategoryId = entity.MenuCategoryId,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable,
            ImageUrl = entity.ImageUrl,
            Summary = entity.Summary
        };
    }
}
