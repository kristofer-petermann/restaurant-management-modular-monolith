using Base.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace WebApp.Tests.Unit.DAL.Active;

public class OrderRepository_Tests
{
    [Fact]
    public async Task GetAllAsync_WithUserFilter_ReturnsOnlyUsersOrders()
    {
        await using var db = await CreateDbAsync();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        db.Orders.Add(MakeOrder(Guid.NewGuid(), userA));
        db.Orders.Add(MakeOrder(Guid.NewGuid(), userB));
        await db.SaveChangesAsync();

        var sut = new OrderRepository(db);
        var result = await sut.GetAllAsync(userA);

        Assert.Single(result);
        Assert.Equal(userA, result[0].UserId);
    }

    [Fact]
    public async Task FindByIdAsync_WithForeignUserFilter_ReturnsNull()
    {
        await using var db = await CreateDbAsync();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        db.Orders.Add(MakeOrder(orderId, ownerId));
        await db.SaveChangesAsync();

        var sut = new OrderRepository(db);
        var result = await sut.FindByIdAsync(orderId, otherUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindTrackedByIdAsync_ReturnsTrackedEntity()
    {
        await using var db = await CreateDbAsync();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Orders.Add(MakeOrder(orderId, userId));
        await db.SaveChangesAsync();

        var sut = new OrderRepository(db);
        var entity = await sut.FindTrackedByIdAsync(orderId);

        Assert.NotNull(entity);
        entity!.OrderStatus = "Updated";
        await db.SaveChangesAsync();

        var reloaded = await db.Orders.FirstAsync(o => o.Id == orderId);
        Assert.Equal("Updated", reloaded.OrderStatus);
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
}
