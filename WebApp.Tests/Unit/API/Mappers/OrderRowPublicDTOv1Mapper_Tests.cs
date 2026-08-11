using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class OrderRowPublicDTOv1Mapper_Tests
{
    private readonly OrderRowPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var bll = new Shared.Contracts.Orders.OrderRowDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 3,
            UnitPrice = 6.75m,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.OrderId.Should().Be(orderId);
        api.MenuItemId.Should().Be(menuItemId);
        api.Quantity.Should().Be(3);
        api.UnitPrice.Should().Be(6.75m);
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.OrderRowResponse
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 3,
            UnitPrice = 6.75m,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.OrderId.Should().Be(orderId);
        bll.MenuItemId.Should().Be(menuItemId);
        bll.Quantity.Should().Be(3);
        bll.UnitPrice.Should().Be(6.75m);
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Orders.OrderRowDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.OrderRowResponse?)null).Should().BeNull();
    }
}

