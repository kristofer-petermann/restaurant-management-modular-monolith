using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuItemService_Tests
{
    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsConcurrency()
    {
        var repo = new Mock<IMenuItemRepository>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuItem?)null);

        var sut = new MenuItemService(repo.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateAsync(new MenuItemDto { Id = Guid.NewGuid(), Name = "Burger" }));
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IMenuItemRepository>();
        var id = Guid.NewGuid();
        var entity = new MenuItem { Id = id, Name = new Base.Domain.LangStr("n") };
        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new MenuItemService(repo.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
