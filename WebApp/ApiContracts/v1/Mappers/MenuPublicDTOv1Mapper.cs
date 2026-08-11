using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class MenuPublicDTOv1Mapper : IBaseMapper<MenuResponse, MenuDto>
{
    public MenuResponse? Map(MenuReadDto? entity)
    {
        if (entity == null) return null;
        return new MenuResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            Summary = entity.Summary
        };
    }

    public MenuResponse? Map(MenuDto? entity)
    {
        if (entity == null) return null;
        return new MenuResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            Summary = entity.Summary
        };
    }

    public MenuDto? Map(MenuResponse? entity)
    {
        if (entity == null) return null;
        return new MenuDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            Name = entity.Name,
            IsActive = entity.IsActive,
            Summary = entity.Summary
        };
    }
}
