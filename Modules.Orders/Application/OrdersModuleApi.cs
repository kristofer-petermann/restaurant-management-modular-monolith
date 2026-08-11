using Modules.Orders.Application.Services;
using Modules.Orders.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;
using Shared.Contracts.Orders;
using Shared.Contracts.Orders.Commands;
using Shared.Contracts.Orders.Results;
using Shared.Contracts.Restaurants;

namespace Modules.Orders.Application;

internal class OrdersModuleApi : IOrdersModuleApi
{
    private readonly OrdersDbContext _dbContext;
    private readonly IOrdersUow _uow;
    private readonly IOrderService _orders;
    private readonly IOrderRowService _orderRows;
    private readonly IPaymentService _payments;
    private readonly IPublisher _publisher;
    private readonly ISender _sender;

    public OrdersModuleApi(
        Modules.Orders.Application.Services.IOrderService orders,
        Modules.Orders.Application.Services.IOrderRowService orderRows,
        Modules.Orders.Application.Services.IPaymentService payments,
        IOrdersUow uow,
        OrdersDbContext dbContext,
        IPublisher publisher,
        ISender sender)
    {
        _orders = orders;
        _orderRows = orderRows;
        _payments = payments;
        _uow = uow;
        _dbContext = dbContext;
        _publisher = publisher;
        _sender = sender;
    }

    public async Task<SubmitOrderResult> SubmitOrderAsync(SubmitOrderCommand command, Guid appUserId = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (appUserId == default) throw new ArgumentException("appUserId must be provided.", nameof(appUserId));
        if (command.Items.Count == 0) throw new ArgumentException("At least one item is required.", nameof(command));
        if (command.Items.Any(i => i.Quantity <= 0)) throw new ArgumentException("Item quantity must be positive.", nameof(command));

        var restaurantExists = await _sender.Send(new ValidateRestaurantExistsQuery(command.RestaurantId));
        if (!restaurantExists) throw new InvalidOperationException("Restaurant does not exist.");
        var restaurantSummary = await _sender.Send(new GetRestaurantSummaryQuery(command.RestaurantId));
        var restaurantNameSnapshot = string.IsNullOrWhiteSpace(restaurantSummary?.RestaurantName)
            ? command.RestaurantId.ToString()
            : restaurantSummary.RestaurantName;

        var menuItemIds = command.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuItemsValid = await _sender.Send(new ValidateMenuItemsQuery(menuItemIds));
        if (!menuItemsValid) throw new InvalidOperationException("One or more menu items are invalid or unavailable.");

        var snapshots = new Dictionary<Guid, MenuItemSnapshotResponse>();
        foreach (var menuItemId in menuItemIds)
        {
            var snapshot = await _sender.Send(new GetMenuItemSnapshotQuery(menuItemId));
            if (snapshot == null || !snapshot.Exists)
            {
                throw new InvalidOperationException($"Menu item '{menuItemId}' is not available.");
            }

            snapshots[menuItemId] = snapshot;
        }

        var totalAmount = command.Items.Sum(item => snapshots[item.MenuItemId].Price * item.Quantity);
        var orderId = Guid.NewGuid();
        var orderSummary = string.IsNullOrWhiteSpace(command.Summary)
            ? $"Order with {command.Items.Count} items"
            : command.Summary;

        _orders.Add(new OrderDto
        {
            Id = orderId,
            UserId = appUserId,
            RestaurantId = command.RestaurantId,
            RestaurantNameSnapshot = restaurantNameSnapshot,
            OrderStatus = "Pending",
            TotalPrice = totalAmount,
            CreatedAt = DateTime.UtcNow,
            PickupTime = command.PickupTime,
            Summary = orderSummary
        }, appUserId);

        foreach (var item in command.Items)
        {
            var snapshot = snapshots[item.MenuItemId];
            _orderRows.Add(new OrderRowDto
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                MenuItemId = item.MenuItemId,
                MenuItemNameSnapshot = snapshot.Name,
                Quantity = item.Quantity,
                UnitPriceSnapshot = snapshot.Price,
                LineTotal = snapshot.Price * item.Quantity,
                Summary = snapshot.Name
            }, appUserId);
        }

        await SaveChangesAsync();
        return new SubmitOrderResult(orderId, totalAmount);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(Guid appUserId = default)
    {
        return await _orders.AllAsync(appUserId);
    }

    public async Task<OrderDto?> GetOrderAsync(Guid id, Guid appUserId = default)
    {
        return await _orders.FindAsync(id, appUserId);
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto entity, Guid appUserId = default)
    {
        _orders.Add(entity, appUserId);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<OrderDto> UpdateOrderDetailsAsync(OrderDto entity, Guid appUserId = default)
    {
        var updated = await _orders.UpdateAsync(entity, appUserId);
        await SaveChangesAsync();
        return updated;
    }

    public async Task DeleteOrderAsync(Guid id, Guid appUserId = default)
    {
        await _orders.RemoveAsync(id, appUserId);
        await SaveChangesAsync();
    }

    public Task<IEnumerable<OrderRowDto>> GetOrderRowsAsync(Guid appUserId = default) => _orderRows.AllAsync(appUserId);
    public Task<OrderRowDto?> FindOrderRowAsync(Guid id, Guid appUserId = default) => _orderRows.FindAsync(id, appUserId);
    public async Task<OrderRowDto> CreateOrderRowAsync(OrderRowDto entity, Guid appUserId = default)
    {
        _orderRows.Add(entity, appUserId);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<OrderRowDto> UpdateOrderRowDetailsAsync(OrderRowDto entity, Guid appUserId = default)
    {
        var updated = await _orderRows.UpdateAsync(entity, appUserId);
        await SaveChangesAsync();
        return updated;
    }

    public async Task DeleteOrderRowAsync(Guid id, Guid appUserId = default)
    {
        await _orderRows.RemoveAsync(id, appUserId);
        await SaveChangesAsync();
    }

    public Task<IEnumerable<PaymentDto>> GetPaymentsAsync(Guid appUserId = default) => _payments.AllAsync(appUserId);
    public Task<PaymentDto?> FindPaymentAsync(Guid id, Guid appUserId = default) => _payments.FindAsync(id, appUserId);
    public async Task<PaymentDto> CreatePaymentAsync(PaymentDto entity, Guid appUserId = default)
    {
        _payments.Add(entity, appUserId);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<PaymentDto> UpdatePaymentDetailsAsync(PaymentDto entity, Guid appUserId = default)
    {
        var updated = await _payments.UpdateAsync(entity, appUserId);
        await SaveChangesAsync();
        return updated;
    }

    public async Task DeletePaymentAsync(Guid id, Guid appUserId = default)
    {
        await _payments.RemoveAsync(id, appUserId);
        await SaveChangesAsync();
    }

    private async Task<int> SaveChangesAsync()
    {
        var createdOrders = _dbContext.ChangeTracker
            .Entries<Order>()
            .Where(x => x.State == EntityState.Added)
            .Select(x => new OrderCreatedEvent(
                x.Entity.Id,
                x.Entity.UserId,
                x.Entity.RestaurantId,
                x.Entity.CreatedAt))
            .ToList();

        var paidPayments = _dbContext.ChangeTracker
            .Entries<Payment>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .Select(x => x.Entity)
            .Where(x =>
                x.PaidAt.HasValue &&
                string.Equals(x.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            .Select(x => new OrderPaidEvent(
                x.OrderId,
                x.Id,
                x.Amount,
                x.PaidAt!.Value))
            .ToList();

        var result = await _uow.SaveChangesAsync();

        foreach (var createdOrder in createdOrders)
        {
            await _publisher.Publish(createdOrder);
        }

        foreach (var paidPayment in paidPayments)
        {
            await _publisher.Publish(paidPayment);
        }

        return result;
    }
}
