using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class OrderPublicDTOv1Mapper_Tests
{
    private readonly OrderPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var pickup = createdAt.AddMinutes(30);
        var bll = new Shared.Contracts.Orders.OrderDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            OrderStatus = "Ready",
            TotalPrice = 19.99m,
            CreatedAt = createdAt,
            PickupTime = pickup,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.UserId.Should().Be(userId);
        api.RestaurantId.Should().Be(restaurantId);
        api.OrderStatus.Should().Be("Ready");
        api.TotalPrice.Should().Be(19.99m);
        api.CreatedAt.Should().Be(createdAt);
        api.PickupTime.Should().Be(pickup);
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var pickup = createdAt.AddMinutes(30);
        var api = new WebApp.ApiContracts.v1.OrderResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            OrderStatus = "Ready",
            TotalPrice = 19.99m,
            CreatedAt = createdAt,
            PickupTime = pickup,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.UserId.Should().Be(userId);
        bll.RestaurantId.Should().Be(restaurantId);
        bll.OrderStatus.Should().Be("Ready");
        bll.TotalPrice.Should().Be(19.99m);
        bll.CreatedAt.Should().Be(createdAt);
        bll.PickupTime.Should().Be(pickup);
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Orders.OrderDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.OrderResponse?)null).Should().BeNull();
    }
}
