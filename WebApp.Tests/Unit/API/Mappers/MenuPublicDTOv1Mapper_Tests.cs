using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class MenuPublicDTOv1Mapper_Tests
{
    private readonly MenuPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var bll = new Shared.Contracts.Restaurants.MenuDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = "Lunch",
            IsActive = true,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.RestaurantId.Should().Be(restaurantId);
        api.Name.Should().Be("Lunch");
        api.IsActive.Should().BeTrue();
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.MenuResponse
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = "Lunch",
            IsActive = true,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.RestaurantId.Should().Be(restaurantId);
        bll.Name.Should().Be("Lunch");
        bll.IsActive.Should().BeTrue();
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.MenuDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.MenuResponse?)null).Should().BeNull();
    }
}

