using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class AddressMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IAddressRepository>();
        var mediator = new Mock<IMediator>();
        var userId = Guid.NewGuid();
        var dto = new AddressDto
        {
            Id = Guid.NewGuid(),
            Street = "Main 1",
            City = "Tallinn",
            PostalCode = "10111",
            Country = "Estonia",
            Summary = "summary",
            UserId = userId,
            RestaurantId = Guid.NewGuid()
        };

        repo.Setup(r => r.AppUserExists(userId)).Returns(true);
        var sut = new AddressService(repo.Object, mediator.Object);
        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<Address>(x =>
            x.Id == dto.Id &&
            x.Street == dto.Street &&
            x.City == dto.City &&
            x.PostalCode == dto.PostalCode &&
            x.Country!.ToString() == dto.Country &&
            x.Summary!.ToString() == dto.Summary &&
            x.UserId == dto.UserId &&
            x.RestaurantId == dto.RestaurantId
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var domain = new Address
        {
            Id = id,
            Street = "Main 1",
            City = "Tallinn",
            PostalCode = "10111",
            Country = new Base.Domain.LangStr("Estonia"),
            Summary = new Base.Domain.LangStr("summary"),
            UserId = userId,
            RestaurantId = restaurantId,
            User = new RestaurantUserProfile { Id = userId, Email = "u@test" },
            Restaurant = new Restaurant { Id = restaurantId, RestaurantName = "R Name" }
        };

        var repo = new Mock<IAddressRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new AddressService(repo.Object, mediator.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal("Main 1", dto.Street);
        Assert.Equal("Tallinn", dto.City);
        Assert.Equal("10111", dto.PostalCode);
        Assert.Equal("Estonia", dto.Country);
        Assert.Equal("summary", dto.Summary);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.NotNull(dto.User);
        Assert.Equal("u@test", dto.User!.Email);
        Assert.NotNull(dto.Restaurant);
        Assert.Equal("R Name", dto.Restaurant!.RestaurantName);
    }
}
