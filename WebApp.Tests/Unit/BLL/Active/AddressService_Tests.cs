using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class AddressService_Tests
{
    [Fact]
    public void Add_BindsUserAndCreatesProjectionUser_WhenMissing()
    {
        var repo = new Mock<IAddressRepository>();
        var mediator = new Mock<IMediator>();
        var boundUserId = Guid.NewGuid();

        repo.Setup(r => r.AppUserExists(boundUserId)).Returns(false);

        var sut = new AddressService(repo.Object, mediator.Object);
        sut.Add(new AddressDto
        {
            Id = Guid.NewGuid(),
            Street = "Main 1",
            City = "Tallinn",
            PostalCode = "10111",
            Country = "EE",
            Summary = "s",
            UserId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid()
        }, boundUserId);

        repo.Verify(r => r.Add(It.Is<Address>(a => a.UserId == boundUserId)), Times.Once);
        repo.Verify(r => r.AddAppUser(It.Is<RestaurantUserProfile>(u => u.Id == boundUserId)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsConcurrency()
    {
        var repo = new Mock<IAddressRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        var sut = new AddressService(repo.Object, mediator.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateAsync(new AddressDto { Id = Guid.NewGuid() }));
    }
}
