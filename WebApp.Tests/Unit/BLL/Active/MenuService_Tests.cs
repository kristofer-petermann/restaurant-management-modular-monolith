using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuService_Tests
{
    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsConcurrency()
    {
        var repo = new Mock<IMenuRepository>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Menu?)null);

        var sut = new MenuService(repo.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateAsync(new MenuDto { Id = Guid.NewGuid(), Name = "Lunch" }));
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IMenuRepository>();
        var id = Guid.NewGuid();
        var entity = new Menu { Id = id, Name = new Base.Domain.LangStr("n") };
        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new MenuService(repo.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
