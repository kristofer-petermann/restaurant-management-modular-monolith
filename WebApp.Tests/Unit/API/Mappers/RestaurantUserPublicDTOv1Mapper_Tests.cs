using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class RestaurantUserPublicDTOv1Mapper_Tests
{
    private readonly RestaurantUserPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bll = new Shared.Contracts.Identity.RestaurantUserDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            UserId = userId,
            UserRole = Shared.Contracts.Identity.RestaurantUserRole.Admin,
            Role = "Owner",
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.RestaurantId.Should().Be(restaurantId);
        api.UserId.Should().Be(userId);
        api.UserRole.Should().Be((int)Shared.Contracts.Identity.RestaurantUserRole.Admin);
        api.Role.Should().Be("Owner");
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.RestaurantUserResponse
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            UserId = userId,
            UserRole = (int)Shared.Contracts.Identity.RestaurantUserRole.Admin,
            Role = "Owner",
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.RestaurantId.Should().Be(restaurantId);
        bll.UserId.Should().Be(userId);
        bll.UserRole.Should().Be(Shared.Contracts.Identity.RestaurantUserRole.Admin);
        bll.Role.Should().Be("Owner");
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Identity.RestaurantUserDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.RestaurantUserResponse?)null).Should().BeNull();
    }
}
