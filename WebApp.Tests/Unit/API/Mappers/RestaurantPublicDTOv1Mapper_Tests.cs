using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class RestaurantPublicDTOv1Mapper_Tests
{
    private readonly RestaurantPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var bll = new Shared.Contracts.Restaurants.RestaurantDto
        {
            Id = Guid.NewGuid(),
            RestaurantName = "Resto",
            Description = "Best food",
            Phone = "+3725555555",
            Email = "info@resto.ee",
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.RestaurantName.Should().Be("Resto");
        api.Description.Should().Be("Best food");
        api.Phone.Should().Be("+3725555555");
        api.Email.Should().Be("info@resto.ee");
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var api = new WebApp.ApiContracts.v1.RestaurantResponse
        {
            Id = Guid.NewGuid(),
            RestaurantName = "Resto",
            Description = "Best food",
            Phone = "+3725555555",
            Email = "info@resto.ee",
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.RestaurantName.Should().Be("Resto");
        bll.Description.Should().Be("Best food");
        bll.Phone.Should().Be("+3725555555");
        bll.Email.Should().Be("info@resto.ee");
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.RestaurantDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.RestaurantResponse?)null).Should().BeNull();
    }
}
