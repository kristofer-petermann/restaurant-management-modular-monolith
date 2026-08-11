using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Infrastructure;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Integration.Orders;

public class OrderRepository_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrderRepository_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Seeded_Order_Is_Available_In_OrdersDbContext()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var order = db.Orders.SingleOrDefault(x => x.Id == TestUsers.OrderAId);
        order.Should().NotBeNull();
        order!.RestaurantId.Should().Be(TestUsers.RestaurantAId);
    }
}
