using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class MenuCategoryService_Tests
{
    [Fact]
    public async Task UpdateAsync_WhenMissing_ThrowsConcurrency()
    {
        var repo = new Mock<IMenuCategoryRepository>();
        repo.Setup(r => r.FindTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MenuCategory?)null);

        var sut = new MenuCategoryService(repo.Object);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            sut.UpdateAsync(new MenuCategoryDto { Id = Guid.NewGuid(), Name = "Mains" }));
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IMenuCategoryRepository>();
        var id = Guid.NewGuid();
        var entity = new MenuCategory { Id = id, Name = new Base.Domain.LangStr("Mains") };
        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new MenuCategoryService(repo.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
