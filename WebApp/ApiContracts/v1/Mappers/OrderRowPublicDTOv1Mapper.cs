using Base.Contracts;
using Shared.Contracts.Orders;

namespace WebApp.ApiContracts.v1.Mappers;

public class OrderRowPublicDTOv1Mapper : IBaseMapper<OrderRowResponse, OrderRowDto>
{
    public OrderRowResponse? Map(OrderRowDto? entity)
    {
        if (entity == null) return null;
        return new OrderRowResponse
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            MenuItemId = entity.MenuItemId,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Summary = entity.Summary
        };
    }

    public OrderRowDto? Map(OrderRowResponse? entity)
    {
        if (entity == null) return null;
        return new OrderRowDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            MenuItemId = entity.MenuItemId,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Summary = entity.Summary ?? string.Empty
        };
    }
}
