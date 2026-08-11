using Base.Domain;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuCategoryMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IMenuCategoryRepository>();
        var sut = new MenuCategoryService(repo.Object);
        var dto = new MenuCategoryDto
        {
            Id = Guid.NewGuid(),
            MenuId = Guid.NewGuid(),
            Name = "Main",
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<MenuCategory>(x =>
            x.Id == dto.Id &&
            x.MenuId == dto.MenuId &&
            x.Name!.ToString() == dto.Name &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var domain = new MenuCategory
        {
            Id = id,
            MenuId = menuId,
            Name = new LangStr("Main"),
            Summary = new LangStr("summary"),
            Menu = new Menu
            {
                Id = menuId,
                Name = new LangStr("Lunch")
            }
        };

        var repo = new Mock<IMenuCategoryRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new MenuCategoryService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(menuId, dto.MenuId);
        Assert.Equal("Main", dto.Name);
        Assert.Equal("summary", dto.Summary);
        Assert.NotNull(dto.Menu);
        Assert.Equal(menuId, dto.Menu!.Id);
        Assert.Equal("Lunch", dto.Menu.Name);
    }
}
