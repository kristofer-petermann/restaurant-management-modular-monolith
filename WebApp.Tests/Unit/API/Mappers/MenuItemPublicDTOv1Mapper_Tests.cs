using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class MenuItemPublicDTOv1Mapper_Tests
{
    private readonly MenuItemPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var categoryId = Guid.NewGuid();
        var bll = new Shared.Contracts.Restaurants.MenuItemDto
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = categoryId,
            Name = "Soup",
            Description = "Hot soup",
            Price = 4.50m,
            IsAvailable = true,
            ImageUrl = "https://example.com/soup.jpg",
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.MenuCategoryId.Should().Be(categoryId);
        api.Name.Should().Be("Soup");
        api.Description.Should().Be("Hot soup");
        api.Price.Should().Be(4.50m);
        api.IsAvailable.Should().BeTrue();
        api.ImageUrl.Should().Be("https://example.com/soup.jpg");
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var categoryId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.MenuItemResponse
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = categoryId,
            Name = "Soup",
            Description = "Hot soup",
            Price = 4.50m,
            IsAvailable = true,
            ImageUrl = "https://example.com/soup.jpg",
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.MenuCategoryId.Should().Be(categoryId);
        bll.Name.Should().Be("Soup");
        bll.Description.Should().Be("Hot soup");
        bll.Price.Should().Be(4.50m);
        bll.IsAvailable.Should().BeTrue();
        bll.ImageUrl.Should().Be("https://example.com/soup.jpg");
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.MenuItemDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.MenuItemResponse?)null).Should().BeNull();
    }
}

