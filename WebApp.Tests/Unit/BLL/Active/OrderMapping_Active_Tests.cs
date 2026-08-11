using Base.Domain;
using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Identity;
using Shared.Contracts.Orders;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class OrderMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IOrderRepository>();
        var mediator = new Mock<IMediator>();
        var restaurantId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.IsAny<GetRestaurantSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantSummaryResponse(restaurantId, "R Name", null));

        var sut = new OrderService(repo.Object, mediator.Object);
        var dto = new OrderDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            OrderStatus = "Pending",
            TotalPrice = 10m,
            CreatedAt = DateTime.UtcNow,
            PickupTime = DateTime.UtcNow.AddHours(1),
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<Order>(x =>
            x.Id == dto.Id &&
            x.UserId == dto.UserId &&
            x.RestaurantId == dto.RestaurantId &&
            x.OrderStatus == dto.OrderStatus &&
            x.TotalPrice == dto.TotalPrice &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var domain = new Order
        {
            Id = id,
            UserId = userId,
            RestaurantId = restaurantId,
            RestaurantNameSnapshot = "R Name",
            OrderStatus = "Pending",
            TotalPrice = 12.5m,
            CreatedAt = DateTime.UtcNow,
            PickupTime = DateTime.UtcNow.AddHours(1),
            Summary = new LangStr("summary")
        };

        var repo = new Mock<IOrderRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);
        mediator.Setup(m => m.Send(It.IsAny<GetUserDisplayNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDisplayNameResponse(userId, "u@test"));

        var sut = new OrderService(repo.Object, mediator.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.Equal("Pending", dto.OrderStatus);
        Assert.Equal(12.5m, dto.TotalPrice);
        Assert.Equal("summary", dto.Summary);
        Assert.Equal("R Name", dto.RestaurantNameSnapshot);
    }
}
