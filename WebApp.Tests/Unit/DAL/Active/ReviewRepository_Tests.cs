using Base.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;

namespace WebApp.Tests.Unit.DAL.Active;

public class ReviewRepository_Tests
{
    [Fact]
    public async Task GetAllAsync_WithUserFilter_ReturnsOnlyUsersReviews()
    {
        await using var db = await CreateDbAsync();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var restaurant = MakeRestaurant(Guid.NewGuid());

        db.Restaurants.Add(restaurant);
        db.Set<RestaurantUserProfile>().AddRange(
            new RestaurantUserProfile { Id = userA, UserName = "a", Email = "a@test" },
            new RestaurantUserProfile { Id = userB, UserName = "b", Email = "b@test" });
        db.Reviews.AddRange(
            MakeReview(Guid.NewGuid(), userA, restaurant.Id, "A"),
            MakeReview(Guid.NewGuid(), userB, restaurant.Id, "B"));
        await db.SaveChangesAsync();

        var sut = new ReviewRepository(db);
        var result = await sut.GetAllAsync(userA);

        Assert.Single(result);
        Assert.Equal(userA, result[0].UserId);
    }

    [Fact]
    public async Task FindTrackedByIdAsync_ReturnsTrackedReview()
    {
        await using var db = await CreateDbAsync();
        var userId = Guid.NewGuid();
        var restaurant = MakeRestaurant(Guid.NewGuid());
        var reviewId = Guid.NewGuid();

        db.Restaurants.Add(restaurant);
        db.Set<RestaurantUserProfile>().Add(new RestaurantUserProfile
        {
            Id = userId,
            UserName = "a",
            Email = "a@test"
        });
        db.Reviews.Add(MakeReview(reviewId, userId, restaurant.Id, "orig"));
        await db.SaveChangesAsync();

        var sut = new ReviewRepository(db);
        var review = await sut.FindTrackedByIdAsync(reviewId);
        Assert.NotNull(review);

        review!.Comment = "updated";
        await db.SaveChangesAsync();

        var reloaded = await db.Reviews.FirstAsync(r => r.Id == reviewId);
        Assert.Equal("updated", reloaded.Comment);
    }

    private static async Task<RestaurantsDbContext> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new RestaurantsDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static Restaurant MakeRestaurant(Guid id) =>
        new()
        {
            Id = id,
            RestaurantName = "R",
            Description = new LangStr("R"),
            Summary = new LangStr("R")
        };

    private static Review MakeReview(Guid id, Guid userId, Guid restaurantId, string comment) =>
        new()
        {
            Id = id,
            UserId = userId,
            RestaurantId = restaurantId,
            Rating = 5,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            Summary = new LangStr("s")
        };
}
