using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class OpeningHourPublicDTOv1Mapper_Tests
{
    private readonly OpeningHourPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var open = new TimeOnly(9, 0);
        var close = new TimeOnly(18, 0);
        var bll = new Shared.Contracts.Restaurants.OpeningHourDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            DayOfWeek = "Monday",
            OpenTime = open,
            CloseTime = close,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.RestaurantId.Should().Be(restaurantId);
        api.DayOfWeek.Should().Be("Monday");
        api.OpenTime.Should().Be(open);
        api.CloseTime.Should().Be(close);
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var open = new TimeOnly(9, 0);
        var close = new TimeOnly(18, 0);
        var api = new WebApp.ApiContracts.v1.OpeningHourResponse
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            DayOfWeek = "Monday",
            OpenTime = open,
            CloseTime = close,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.RestaurantId.Should().Be(restaurantId);
        bll.DayOfWeek.Should().Be("Monday");
        bll.OpenTime.Should().Be(open);
        bll.CloseTime.Should().Be(close);
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.OpeningHourDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.OpeningHourResponse?)null).Should().BeNull();
    }
}
