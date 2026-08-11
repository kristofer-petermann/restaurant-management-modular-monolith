using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class ReviewPublicDTOv1Mapper_Tests
{
    private readonly ReviewPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var bll = new Shared.Contracts.Restaurants.ReviewDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            Rating = 5,
            Comment = "Excellent",
            CreatedAt = createdAt,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.UserId.Should().Be(userId);
        api.RestaurantId.Should().Be(restaurantId);
        api.Rating.Should().Be(5);
        api.Comment.Should().Be("Excellent");
        api.CreatedAt.Should().Be(createdAt);
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var api = new WebApp.ApiContracts.v1.ReviewResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            Rating = 5,
            Comment = "Excellent",
            CreatedAt = createdAt,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.UserId.Should().Be(userId);
        bll.RestaurantId.Should().Be(restaurantId);
        bll.Rating.Should().Be(5);
        bll.Comment.Should().Be("Excellent");
        bll.CreatedAt.Should().Be(createdAt);
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.ReviewDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.ReviewResponse?)null).Should().BeNull();
    }
}
