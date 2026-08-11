namespace Shared.Contracts.Orders.Commands;

public sealed record SubmitOrderCommand(
    Guid RestaurantId,
    IReadOnlyList<SubmitOrderItemCommand> Items,
    DateTime? PickupTime,
    string? Summary);

public sealed record SubmitOrderItemCommand(
    Guid MenuItemId,
    int Quantity);
