using Base.Contracts;
using Shared.Contracts.Restaurants;

namespace WebApp.ApiContracts.v1.Mappers;

public class AddressPublicDTOv1Mapper : IBaseMapper<AddressResponse, AddressDto>
{
    public AddressResponse? Map(AddressReadDto? entity)
    {
        if (entity == null) return null;
        return new AddressResponse
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            Summary = entity.Summary,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId
        };
    }

    public AddressResponse? Map(AddressDto? entity)
    {
        if (entity == null) return null;
        return new AddressResponse
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            Summary = entity.Summary,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId
        };
    }

    public AddressDto? Map(AddressResponse? entity)
    {
        if (entity == null) return null;
        return new AddressDto
        {
            Id = entity.Id,
            Street = entity.Street,
            City = entity.City,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            Summary = entity.Summary,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId
        };
    }
}
