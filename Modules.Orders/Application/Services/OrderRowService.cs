using MediatR;
using Shared.Contracts.Identity;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Orders;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace Modules.Orders.Application.Services;

internal class OrderRowService : Modules.Orders.Application.Services.IOrderRowService
{
    private readonly IOrderRowRepository _orderRows;
    private readonly IMediator _mediator;

    public OrderRowService(IOrderRowRepository orderRows, IMediator mediator)
    {
        _orderRows = orderRows;
        _mediator = mediator;
    }

    public async Task<IEnumerable<OrderRowDto>> AllAsync(Guid appUserId = default)
    {
        var entities = await _orderRows.GetAllAsync(appUserId);
        var menuItemNames = await ResolveMenuItemNamesAsync(entities.Select(e => e.MenuItemId));
        return entities.Select(e => ToDto(e, menuItemNames)).ToList();
    }

    public async Task<OrderRowDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _orderRows.FindByIdAsync(id, appUserId);
        if (entity == null) return null;

        var menuItemNames = await ResolveMenuItemNamesAsync(new[] { entity.MenuItemId });
        return ToDto(entity, menuItemNames);
    }

    public void Add(OrderRowDto entity, Guid appUserId = default)
    {
        _orderRows.Add(ToDomain(entity));
    }

    public async Task<OrderRowDto> UpdateAsync(OrderRowDto entity, Guid appUserId = default)
    {
        var dbEntity = await _orderRows.FindTrackedByIdAsync(entity.Id) ?? throw new InvalidOperationException("Order row not found");
        dbEntity.OrderId = entity.OrderId;
        dbEntity.MenuItemId = entity.MenuItemId;
        dbEntity.MenuItemNameSnapshot = string.IsNullOrWhiteSpace(entity.MenuItemNameSnapshot)
            ? ResolveMenuItemName(entity.MenuItemId)
            : entity.MenuItemNameSnapshot!;
        dbEntity.Quantity = entity.Quantity;
        dbEntity.UnitPriceSnapshot = entity.UnitPriceSnapshot;
        dbEntity.LineTotal = entity.Quantity * entity.UnitPriceSnapshot;
        dbEntity.Summary = entity.Summary ?? string.Empty;
        return ToDto(dbEntity, new Dictionary<Guid, string>());
    }

    public void Remove(OrderRowDto entity)
    {
        _orderRows.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _orderRows.FindForDeleteAsync(id, appUserId);
        if (entity != null) _orderRows.Remove(entity);
    }

    private OrderRowDto ToDto(OrderRow entity, IReadOnlyDictionary<Guid, string> menuItemNames)
    {
        var userDisplayName = entity.Order == null ? null : ResolveUserDisplayName(entity.Order.UserId);
        return new OrderRowDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            MenuItemId = entity.MenuItemId,
            Quantity = entity.Quantity,
            UnitPriceSnapshot = entity.UnitPriceSnapshot,
            MenuItemNameSnapshot = entity.MenuItemNameSnapshot,
            LineTotal = entity.LineTotal,
            Summary = entity.Summary,
            Order = entity.Order == null
                ? null
                : new OrderRowOrderDto
                {
                    Id = entity.Order.Id,
                    UserId = entity.Order.UserId,
                    CreatedAt = entity.Order.CreatedAt,
                    User = new OrderRowUserDto
                    {
                        Id = entity.Order.UserId,
                        Email = userDisplayName
                    }
                },
            MenuItem = new OrderRowMenuItemDto
            {
                Id = entity.MenuItemId,
                Name = entity.MenuItemNameSnapshot
                    ?? (menuItemNames.TryGetValue(entity.MenuItemId, out var menuItemName) ? menuItemName : null)
            }
        };
    }

    private OrderRow ToDomain(OrderRowDto entity)
    {
        return new OrderRow
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            MenuItemId = entity.MenuItemId,
            MenuItemNameSnapshot = string.IsNullOrWhiteSpace(entity.MenuItemNameSnapshot)
                ? ResolveMenuItemName(entity.MenuItemId)
                : entity.MenuItemNameSnapshot!,
            Quantity = entity.Quantity,
            UnitPriceSnapshot = entity.UnitPriceSnapshot,
            LineTotal = entity.LineTotal == default
                ? entity.Quantity * entity.UnitPriceSnapshot
                : entity.LineTotal,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private string ResolveMenuItemName(Guid menuItemId)
    {
        var snapshot = _mediator.Send(new GetMenuItemSnapshotQuery(menuItemId)).GetAwaiter().GetResult();
        var snapshotName = snapshot?.Name;
        return IsMissingDisplayName(snapshotName) ? menuItemId.ToString() : snapshotName!;
    }

    private string ResolveUserDisplayName(Guid userId)
    {
        var response = _mediator.Send(new GetUserDisplayNameQuery(userId)).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(response?.DisplayName) ? userId.ToString() : response.DisplayName;
    }

    private static bool IsMissingDisplayName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<Guid, string>> ResolveMenuItemNamesAsync(IEnumerable<Guid> menuItemIds)
    {
        var map = new Dictionary<Guid, string>();
        foreach (var id in menuItemIds.Distinct())
        {
            var snapshot = await _mediator.Send(new GetMenuItemSnapshotQuery(id));
            if (snapshot != null && !IsMissingDisplayName(snapshot.Name))
            {
                map[id] = snapshot.Name;
            }
        }

        return map;
    }
}
