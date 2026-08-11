using Shared.Contracts.Identity;
using MediatR;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Orders;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace Modules.Orders.Application.Services;

internal class OrderService : Modules.Orders.Application.Services.IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IMediator _mediator;

    public OrderService(IOrderRepository orders, IMediator mediator)
    {
        _orders = orders;
        _mediator = mediator;
    }

    public async Task<IEnumerable<OrderDto>> AllAsync(Guid appUserId = default)
    {
        return (await _orders.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<OrderDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _orders.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public void Add(OrderDto entity, Guid appUserId = default)
    {
        var domain = ToDomain(entity);
        domain.UserId = appUserId == default ? domain.UserId : appUserId;
        if (string.IsNullOrWhiteSpace(domain.RestaurantNameSnapshot))
        {
            domain.RestaurantNameSnapshot = ResolveRestaurantName(domain.RestaurantId);
        }
        _orders.Add(domain);
    }

    public async Task<OrderDto> UpdateAsync(OrderDto entity, Guid appUserId = default)
    {
        var dbEntity = await _orders.FindTrackedByIdAsync(entity.Id) ?? throw new InvalidOperationException("Order not found");
        dbEntity.UserId = appUserId == default ? entity.UserId : appUserId;
        dbEntity.RestaurantId = entity.RestaurantId;
        dbEntity.RestaurantNameSnapshot = string.IsNullOrWhiteSpace(entity.RestaurantNameSnapshot)
            ? ResolveRestaurantName(entity.RestaurantId)
            : entity.RestaurantNameSnapshot;
        dbEntity.OrderStatus = entity.OrderStatus;
        dbEntity.TotalPrice = entity.TotalPrice;
        dbEntity.CreatedAt = entity.CreatedAt;
        dbEntity.PickupTime = entity.PickupTime ?? entity.CreatedAt;
        dbEntity.Summary = entity.Summary ?? string.Empty;
        return ToDto(dbEntity);
    }

    public void Remove(OrderDto entity)
    {
        _orders.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _orders.FindForDeleteAsync(id, appUserId);
        if (entity != null) _orders.Remove(entity);
    }

    private OrderDto ToDto(Order entity)
    {
        var userDisplayName = ResolveUserDisplayName(entity.UserId);
        return new OrderDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            OrderStatus = entity.OrderStatus,
            TotalPrice = entity.TotalPrice,
            CreatedAt = entity.CreatedAt,
            PickupTime = entity.PickupTime,
            Summary = entity.Summary,
            RestaurantNameSnapshot = entity.RestaurantNameSnapshot,
            UserEmail = userDisplayName,
            User = new OrderUserDto
            {
                Id = entity.UserId,
                Email = userDisplayName
            },
            Restaurant = new OrderRestaurantDto
            {
                Id = entity.RestaurantId,
                RestaurantName = entity.RestaurantNameSnapshot
            }
        };
    }

    private static Order ToDomain(OrderDto entity)
    {
        return new Order
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RestaurantId = entity.RestaurantId,
            RestaurantNameSnapshot = string.IsNullOrWhiteSpace(entity.RestaurantNameSnapshot)
                ? entity.Restaurant?.RestaurantName ?? string.Empty
                : entity.RestaurantNameSnapshot,
            OrderStatus = entity.OrderStatus,
            TotalPrice = entity.TotalPrice,
            CreatedAt = entity.CreatedAt,
            PickupTime = entity.PickupTime ?? entity.CreatedAt,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private string ResolveRestaurantName(Guid restaurantId)
    {
        var summary = _mediator.Send(new GetRestaurantSummaryQuery(restaurantId)).GetAwaiter().GetResult();
        var name = summary?.RestaurantName;
        return string.IsNullOrWhiteSpace(name) ? restaurantId.ToString() : name;
    }

    private string ResolveUserDisplayName(Guid userId)
    {
        var response = _mediator.Send(new GetUserDisplayNameQuery(userId)).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(response?.DisplayName) ? userId.ToString() : response.DisplayName;
    }
}
