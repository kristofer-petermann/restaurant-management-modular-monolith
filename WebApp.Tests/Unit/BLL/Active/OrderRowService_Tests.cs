using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Orders;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class OrderRowService_Tests
{
    [Fact]
    public void Add_ResolvesMenuItemName_WhenMissing()
    {
        var repo = new Mock<IOrderRowRepository>();
        var mediator = new Mock<IMediator>();
        var menuItemId = Guid.NewGuid();

        mediator
            .Setup(m => m.Send(It.IsAny<GetMenuItemSnapshotQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MenuItemSnapshotResponse(menuItemId, "Burger", 8m, true));

        var sut = new OrderRowService(repo.Object, mediator.Object);
        sut.Add(new OrderRowDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPriceSnapshot = 8m,
            Summary = "s"
        });

        repo.Verify(r => r.Add(It.Is<OrderRow>(x =>
            x.MenuItemId == menuItemId &&
            x.MenuItemNameSnapshot == "Burger" &&
            x.LineTotal == 16m)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_Throws()
    {
        var repo = new Mock<IOrderRowRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderRow?)null);

        var sut = new OrderRowService(repo.Object, mediator.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UpdateAsync(new OrderRowDto { Id = Guid.NewGuid() }));
    }
}
