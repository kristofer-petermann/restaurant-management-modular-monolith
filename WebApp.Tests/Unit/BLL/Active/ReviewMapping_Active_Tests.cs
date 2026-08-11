using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class ReviewMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AndBindsUser()
    {
        var repo = new Mock<IReviewRepository>();
        var boundUserId = Guid.NewGuid();
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great",
            CreatedAt = DateTime.UtcNow,
            Summary = "summary"
        };

        var sut = new ReviewService(repo.Object);
        sut.Add(dto, boundUserId);

        repo.Verify(r => r.Add(It.Is<Review>(x =>
            x.Id == dto.Id &&
            x.UserId == boundUserId &&
            x.RestaurantId == dto.RestaurantId &&
            x.Rating == dto.Rating &&
            x.Comment == dto.Comment &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var domain = new Review
        {
            Id = id,
            UserId = userId,
            RestaurantId = restaurantId,
            Rating = 4,
            Comment = "Good",
            CreatedAt = DateTime.UtcNow,
            Summary = new Base.Domain.LangStr("summary"),
            AppUser = new RestaurantUserProfile { Id = userId, Email = "u@test" },
            Restaurant = new Restaurant { Id = restaurantId, RestaurantName = "R Name" }
        };

        var repo = new Mock<IReviewRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new ReviewService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.Equal(4, dto.Rating);
        Assert.Equal("Good", dto.Comment);
        Assert.Equal("summary", dto.Summary);
        Assert.NotNull(dto.AppUser);
        Assert.Equal("u@test", dto.AppUser!.Email);
        Assert.NotNull(dto.Restaurant);
        Assert.Equal("R Name", dto.Restaurant!.RestaurantName);
    }
}
