using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Orders;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class OrderService_Tests
{
    [Fact]
    public void Add_UsesBoundUserAndResolvesRestaurantNameWhenMissing()
    {
        var repo = new Mock<IOrderRepository>();
        var mediator = new Mock<IMediator>();
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        mediator
            .Setup(m => m.Send(It.IsAny<GetRestaurantSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantSummaryResponse(restaurantId, "R Name", null));

        var sut = new OrderService(repo.Object, mediator.Object);
        sut.Add(new OrderDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            OrderStatus = "Pending",
            TotalPrice = 10m,
            CreatedAt = DateTime.UtcNow,
            PickupTime = DateTime.UtcNow.AddHours(1),
            Summary = "s",
            RestaurantNameSnapshot = ""
        }, userId);

        repo.Verify(r => r.Add(It.Is<Order>(o =>
            o.UserId == userId &&
            o.RestaurantNameSnapshot == "R Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_Throws()
    {
        var repo = new Mock<IOrderRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var sut = new OrderService(repo.Object, mediator.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UpdateAsync(new OrderDto { Id = Guid.NewGuid() }));
    }
}
