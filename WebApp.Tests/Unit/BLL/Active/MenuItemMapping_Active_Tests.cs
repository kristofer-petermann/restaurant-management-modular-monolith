using Base.Domain;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuItemMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IMenuItemRepository>();
        var sut = new MenuItemService(repo.Object);
        var dto = new MenuItemDto
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = Guid.NewGuid(),
            Name = "Burger",
            Description = "Beef burger",
            Price = 12.50m,
            IsAvailable = true,
            ImageUrl = "https://img.test/burger.jpg",
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<MenuItem>(x =>
            x.Id == dto.Id &&
            x.MenuCategoryId == dto.MenuCategoryId &&
            x.Name!.ToString() == dto.Name &&
            x.Description!.ToString() == dto.Description &&
            x.Price == dto.Price &&
            x.IsAvailable == dto.IsAvailable &&
            x.ImageUrl == dto.ImageUrl &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var menuCategoryId = Guid.NewGuid();
        var domain = new MenuItem
        {
            Id = id,
            MenuCategoryId = menuCategoryId,
            Name = new LangStr("Burger"),
            Description = new LangStr("Beef burger"),
            Price = 12.50m,
            IsAvailable = true,
            ImageUrl = "https://img.test/burger.jpg",
            Summary = new LangStr("summary"),
            MenuCategory = new MenuCategory
            {
                Id = menuCategoryId,
                Name = new LangStr("Main")
            }
        };

        var repo = new Mock<IMenuItemRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new MenuItemService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(menuCategoryId, dto.MenuCategoryId);
        Assert.Equal("Burger", dto.Name);
        Assert.Equal("Beef burger", dto.Description);
        Assert.Equal(12.50m, dto.Price);
        Assert.True(dto.IsAvailable);
        Assert.Equal("https://img.test/burger.jpg", dto.ImageUrl);
        Assert.Equal("summary", dto.Summary);
        Assert.NotNull(dto.MenuCategory);
        Assert.Equal(menuCategoryId, dto.MenuCategory!.Id);
        Assert.Equal("Main", dto.MenuCategory.Name);
    }
}
