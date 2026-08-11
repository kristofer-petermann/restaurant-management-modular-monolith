using Base.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace WebApp.Tests.Unit.DAL.Active;

public class OrderRowRepository_Tests
{
    [Fact]
    public async Task GetAllAsync_WithUserFilter_ReturnsOnlyUsersRows()
    {
        await using var db = await CreateDbAsync();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var orderA = MakeOrder(Guid.NewGuid(), userA);
        var orderB = MakeOrder(Guid.NewGuid(), userB);
        db.Orders.AddRange(orderA, orderB);
        db.OrderRows.AddRange(
            MakeRow(Guid.NewGuid(), orderA.Id, "A"),
            MakeRow(Guid.NewGuid(), orderB.Id, "B"));
        await db.SaveChangesAsync();

        var sut = new OrderRowRepository(db);
        var rows = await sut.GetAllAsync(userA);

        Assert.Single(rows);
        Assert.Equal(orderA.Id, rows[0].OrderId);
    }

    [Fact]
    public async Task FindTrackedByIdAsync_ReturnsTrackedEntity()
    {
        await using var db = await CreateDbAsync();
        var order = MakeOrder(Guid.NewGuid(), Guid.NewGuid());
        var rowId = Guid.NewGuid();
        db.Orders.Add(order);
        db.OrderRows.Add(MakeRow(rowId, order.Id, "orig"));
        await db.SaveChangesAsync();

        var sut = new OrderRowRepository(db);
        var row = await sut.FindTrackedByIdAsync(rowId);
        Assert.NotNull(row);

        row!.MenuItemNameSnapshot = "updated";
        await db.SaveChangesAsync();

        var reloaded = await db.OrderRows.FirstAsync(r => r.Id == rowId);
        Assert.Equal("updated", reloaded.MenuItemNameSnapshot);
    }

    private static async Task<OrdersDbContext> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new OrdersDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static Order MakeOrder(Guid id, Guid userId) =>
        new()
        {
            Id = id,
            UserId = userId,
            RestaurantId = Guid.NewGuid(),
            RestaurantNameSnapshot = "R",
            OrderStatus = "Pending",
            TotalPrice = 10m,
            CreatedAt = DateTime.UtcNow,
            PickupTime = DateTime.UtcNow.AddHours(1),
            Summary = new LangStr("s")
        };

    private static OrderRow MakeRow(Guid id, Guid orderId, string name) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            MenuItemId = Guid.NewGuid(),
            MenuItemNameSnapshot = name,
            Quantity = 1,
            UnitPriceSnapshot = 10m,
            LineTotal = 10m,
            Summary = new LangStr("s")
        };
}
