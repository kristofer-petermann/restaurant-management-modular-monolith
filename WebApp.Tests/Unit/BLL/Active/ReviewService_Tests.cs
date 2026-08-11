using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class ReviewService_Tests
{
    [Fact]
    public void Add_BindsUserIdFromAppUserContext()
    {
        var repo = new Mock<IReviewRepository>();
        var boundUserId = Guid.NewGuid();

        var sut = new ReviewService(repo.Object);
        sut.Add(new ReviewDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            Rating = 5,
            Comment = "ok",
            CreatedAt = DateTime.UtcNow,
            Summary = "s"
        }, boundUserId);

        repo.Verify(r => r.Add(It.Is<Review>(x => x.UserId == boundUserId)), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IReviewRepository>();
        var id = Guid.NewGuid();
        var entity = new Review { Id = id };
        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new ReviewService(repo.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
