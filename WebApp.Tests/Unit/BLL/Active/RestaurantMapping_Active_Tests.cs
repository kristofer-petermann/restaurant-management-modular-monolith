using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class RestaurantMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IRestaurantRepository>();
        var sut = new RestaurantService(repo.Object);
        var dto = new RestaurantDto
        {
            Id = Guid.NewGuid(),
            RestaurantName = "Test Resto",
            Description = "Nice place",
            Phone = "+3720000000",
            Email = "resto@test.local",
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<Restaurant>(x =>
            x.Id == dto.Id &&
            x.RestaurantName == dto.RestaurantName &&
            x.Description!.ToString() == dto.Description &&
            x.Phone == dto.Phone &&
            x.Email == dto.Email &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var domain = new Restaurant
        {
            Id = id,
            RestaurantName = "Test Resto",
            Description = new Base.Domain.LangStr("Nice place"),
            Phone = "+3720000000",
            Email = "resto@test.local",
            Summary = new Base.Domain.LangStr("summary")
        };
        var repo = new Mock<IRestaurantRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new RestaurantService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(domain.Id, dto!.Id);
        Assert.Equal("Test Resto", dto.RestaurantName);
        Assert.Equal("Nice place", dto.Description);
        Assert.Equal("+3720000000", dto.Phone);
        Assert.Equal("resto@test.local", dto.Email);
        Assert.Equal("summary", dto.Summary);
    }
}
