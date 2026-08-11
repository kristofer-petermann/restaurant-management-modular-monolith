using Base.Contracts;
using Shared.Contracts.Orders;

namespace WebApp.ApiContracts.v1.Mappers;

public class OrderPublicDTOv1Mapper : IBaseMapper<OrderResponse, OrderDto>
{
    public OrderResponse? Map(OrderDto? entity)
    {
        if (entity == null) return null;
        return new OrderResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            OrderStatus = entity.OrderStatus,
            TotalPrice = entity.TotalPrice,
            CreatedAt = entity.CreatedAt,
            PickupTime = entity.PickupTime ?? entity.CreatedAt,
            Summary = entity.Summary
        };
    }

    public OrderDto? Map(OrderResponse? entity)
    {
        if (entity == null) return null;
        return new OrderDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            OrderStatus = entity.OrderStatus,
            TotalPrice = entity.TotalPrice,
            CreatedAt = entity.CreatedAt,
            PickupTime = entity.PickupTime,
            Summary = entity.Summary ?? string.Empty
        };
    }
}
