using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.API.Mappers;

public class AddressPublicDTOv1Mapper_Tests
{
    private readonly AddressPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var bll = new AddressDto
        {
            Id = Guid.NewGuid(),
            Street = "Main 1",
            City = "Tallinn",
            PostalCode = "10111",
            Country = "EE",
            Summary = "summary",
            UserId = userId,
            RestaurantId = restaurantId
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.Street.Should().Be("Main 1");
        api.City.Should().Be("Tallinn");
        api.PostalCode.Should().Be("10111");
        api.Country.Should().Be("EE");
        api.Summary.Should().Be("summary");
        api.UserId.Should().Be(userId);
        api.RestaurantId.Should().Be(restaurantId);
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.AddressResponse
        {
            Id = Guid.NewGuid(),
            Street = "Main 1",
            City = "Tallinn",
            PostalCode = "10111",
            Country = "EE",
            Summary = "summary",
            UserId = userId,
            RestaurantId = restaurantId
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.Street.Should().Be("Main 1");
        bll.City.Should().Be("Tallinn");
        bll.PostalCode.Should().Be("10111");
        bll.Country.Should().Be("EE");
        bll.Summary.Should().Be("summary");
        bll.UserId.Should().Be(userId);
        bll.RestaurantId.Should().Be(restaurantId);
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((AddressDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.AddressResponse?)null).Should().BeNull();
    }
}
