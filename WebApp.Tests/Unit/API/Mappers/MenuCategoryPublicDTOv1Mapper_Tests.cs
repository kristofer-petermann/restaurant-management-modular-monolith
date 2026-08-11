using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class MenuCategoryPublicDTOv1Mapper_Tests
{
    private readonly MenuCategoryPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var menuId = Guid.NewGuid();
        var bll = new Shared.Contracts.Restaurants.MenuCategoryDto
        {
            Id = Guid.NewGuid(),
            MenuId = menuId,
            Name = "Starters",
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.MenuId.Should().Be(menuId);
        api.Name.Should().Be("Starters");
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var menuId = Guid.NewGuid();
        var api = new WebApp.ApiContracts.v1.MenuCategoryResponse
        {
            Id = Guid.NewGuid(),
            MenuId = menuId,
            Name = "Starters",
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.MenuId.Should().Be(menuId);
        bll.Name.Should().Be("Starters");
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Restaurants.MenuCategoryDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.MenuCategoryResponse?)null).Should().BeNull();
    }
}

