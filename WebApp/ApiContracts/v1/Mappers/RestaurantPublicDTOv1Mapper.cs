using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class RestaurantPublicDTOv1Mapper : IBaseMapper<RestaurantResponse, RestaurantDto>
{
    public RestaurantResponse? Map(RestaurantReadDto? entity)
    {
        if (entity == null) return null;
        return new RestaurantResponse
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary
        };
    }

    public RestaurantResponse? Map(RestaurantDto? entity)
    {
        if (entity == null) return null;
        return new RestaurantResponse
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary
        };
    }

    public RestaurantDto? Map(RestaurantResponse? entity)
    {
        if (entity == null) return null;
        return new RestaurantDto
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary
        };
    }
}
