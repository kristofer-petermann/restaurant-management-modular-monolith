using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class RestaurantService_Tests
{
    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsConcurrency()
    {
        var repo = new Mock<IRestaurantRepository>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Restaurant?)null);

        var sut = new RestaurantService(repo.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateAsync(new RestaurantDto { Id = Guid.NewGuid(), RestaurantName = "R" }));
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IRestaurantRepository>();
        var id = Guid.NewGuid();
        var entity = new Restaurant { Id = id, RestaurantName = "R" };
        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new RestaurantService(repo.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
