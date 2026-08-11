using Base.Contracts;
using Shared.Contracts.Identity;

namespace WebApp.ApiContracts.v1.Mappers;

public class RestaurantUserPublicDTOv1Mapper : IBaseMapper<RestaurantUserResponse, RestaurantUserDto>
{
    public RestaurantUserResponse? Map(RestaurantUserReadDto? entity)
    {
        if (entity == null) return null;
        return new RestaurantUserResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            RestaurantName = entity.RestaurantName,
            UserId = entity.UserId,
            UserEmail = entity.UserEmail,
            UserRole = (int)entity.UserRole,
            Role = entity.Role,
            Summary = entity.Summary
        };
    }

    public RestaurantUserResponse? Map(RestaurantUserDto? entity)
    {
        if (entity == null) return null;
        return new RestaurantUserResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            UserId = entity.UserId,
            UserRole = (int)entity.UserRole,
            Role = entity.Role,
            Summary = entity.Summary
        };
    }

    public RestaurantUserDto? Map(RestaurantUserResponse? entity)
    {
        if (entity == null) return null;
        return new RestaurantUserDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            UserId = entity.UserId,
            UserRole = (RestaurantUserRole)entity.UserRole,
            Role = entity.Role,
            Summary = entity.Summary
        };
    }
}
