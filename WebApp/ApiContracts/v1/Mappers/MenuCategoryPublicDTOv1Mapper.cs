using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class MenuCategoryPublicDTOv1Mapper : IBaseMapper<MenuCategoryResponse, MenuCategoryDto>
{
    public MenuCategoryResponse? Map(MenuCategoryReadDto? entity)
    {
        if (entity == null) return null;
        return new MenuCategoryResponse
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name,
            Summary = entity.Summary
        };
    }

    public MenuCategoryResponse? Map(MenuCategoryDto? entity)
    {
        if (entity == null) return null;
        return new MenuCategoryResponse
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name,
            Summary = entity.Summary
        };
    }

    public MenuCategoryDto? Map(MenuCategoryResponse? entity)
    {
        if (entity == null) return null;
        return new MenuCategoryDto
        {
            Id = entity.Id,
            MenuId = entity.MenuId,
            Name = entity.Name,
            Summary = entity.Summary
        };
    }
}
