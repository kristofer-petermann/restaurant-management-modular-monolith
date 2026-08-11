using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class OpeningHourPublicDTOv1Mapper : IBaseMapper<OpeningHourResponse, OpeningHourDto>
{
    public OpeningHourResponse? Map(OpeningHourReadDto? entity)
    {
        if (entity == null) return null;
        return new OpeningHourResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary
        };
    }

    public OpeningHourResponse? Map(OpeningHourDto? entity)
    {
        if (entity == null) return null;
        return new OpeningHourResponse
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary
        };
    }

    public OpeningHourDto? Map(OpeningHourResponse? entity)
    {
        if (entity == null) return null;
        return new OpeningHourDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary
        };
    }
}
