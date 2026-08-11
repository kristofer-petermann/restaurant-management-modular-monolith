using Base.Domain;
using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Orders;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class OrderRowMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IOrderRowRepository>();
        var mediator = new Mock<IMediator>();
        var menuItemId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.IsAny<GetMenuItemSnapshotQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MenuItemSnapshotResponse(menuItemId, "Burger", 10m, true));

        var sut = new OrderRowService(repo.Object, mediator.Object);
        var dto = new OrderRowDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPriceSnapshot = 10m,
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<OrderRow>(x =>
            x.Id == dto.Id &&
            x.OrderId == dto.OrderId &&
            x.MenuItemId == dto.MenuItemId &&
            x.Quantity == dto.Quantity &&
            x.UnitPriceSnapshot == dto.UnitPriceSnapshot &&
            x.LineTotal == 20m
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var domain = new OrderRow
        {
            Id = id,
            OrderId = Guid.NewGuid(),
            MenuItemId = Guid.NewGuid(),
            MenuItemNameSnapshot = "Burger",
            Quantity = 2,
            UnitPriceSnapshot = 10m,
            LineTotal = 20m,
            Summary = new LangStr("summary")
        };

        var repo = new Mock<IOrderRowRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new OrderRowService(repo.Object, mediator.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(domain.OrderId, dto.OrderId);
        Assert.Equal(domain.MenuItemId, dto.MenuItemId);
        Assert.Equal("Burger", dto.MenuItemNameSnapshot);
        Assert.Equal(2, dto.Quantity);
        Assert.Equal(10m, dto.UnitPriceSnapshot);
        Assert.Equal(20m, dto.LineTotal);
    }
}
