using Base.Domain;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IMenuRepository>();
        var sut = new MenuService(repo.Object);
        var dto = new MenuDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            Name = "Lunch",
            IsActive = true,
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<Menu>(x =>
            x.Id == dto.Id &&
            x.RestaurantId == dto.RestaurantId &&
            x.Name!.ToString() == dto.Name &&
            x.IsActive == dto.IsActive &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var domain = new Menu
        {
            Id = id,
            RestaurantId = restaurantId,
            Name = new LangStr("Lunch"),
            IsActive = true,
            Summary = new LangStr("summary"),
            Restaurant = new Restaurant { Id = restaurantId, RestaurantName = "R Name" }
        };

        var repo = new Mock<IMenuRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new MenuService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.Equal("Lunch", dto.Name);
        Assert.True(dto.IsActive);
        Assert.Equal("summary", dto.Summary);
        Assert.NotNull(dto.Restaurant);
        Assert.Equal("R Name", dto.Restaurant!.RestaurantName);
    }
}
