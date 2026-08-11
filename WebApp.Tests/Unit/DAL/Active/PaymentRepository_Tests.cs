using Base.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace WebApp.Tests.Unit.DAL.Active;

public class PaymentRepository_Tests
{
    [Fact]
    public async Task GetAllAsync_WithUserFilter_ReturnsOnlyUsersPayments()
    {
        await using var db = await CreateDbAsync();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var orderA = MakeOrder(Guid.NewGuid(), userA);
        var orderB = MakeOrder(Guid.NewGuid(), userB);
        db.Orders.AddRange(orderA, orderB);
        db.Payments.AddRange(
            MakePayment(Guid.NewGuid(), orderA.Id, 10m),
            MakePayment(Guid.NewGuid(), orderB.Id, 20m));
        await db.SaveChangesAsync();

        var sut = new PaymentRepository(db);
        var payments = await sut.GetAllAsync(userA);

        Assert.Single(payments);
        Assert.Equal(orderA.Id, payments[0].OrderId);
    }

    [Fact]
    public async Task FindTrackedByIdAsync_ReturnsTrackedEntity()
    {
        await using var db = await CreateDbAsync();
        var order = MakeOrder(Guid.NewGuid(), Guid.NewGuid());
        var paymentId = Guid.NewGuid();
        db.Orders.Add(order);
        db.Payments.Add(MakePayment(paymentId, order.Id, 10m));
        await db.SaveChangesAsync();

        var sut = new PaymentRepository(db);
        var payment = await sut.FindTrackedByIdAsync(paymentId);
        Assert.NotNull(payment);

        payment!.PaymentStatus = "Paid";
        await db.SaveChangesAsync();

        var reloaded = await db.Payments.FirstAsync(p => p.Id == paymentId);
        Assert.Equal("Paid", reloaded.PaymentStatus);
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

    private static Payment MakePayment(Guid id, Guid orderId, decimal amount) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            Amount = amount,
            PaymentStatus = "Pending",
            PaymentMethod = "Card",
            Summary = new LangStr("s")
        };
}
