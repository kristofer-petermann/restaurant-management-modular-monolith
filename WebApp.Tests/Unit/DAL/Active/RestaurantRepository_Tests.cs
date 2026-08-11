using Base.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;

namespace WebApp.Tests.Unit.DAL.Active;

public class RestaurantRepository_Tests
{
    [Fact]
    public async Task GetAllAsync_WithUserFilter_ReturnsOnlyOwnedRestaurants()
    {
        await using var db = await CreateDbAsync();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        db.Set<RestaurantUserProfile>().AddRange(
            new RestaurantUserProfile { Id = userA, UserName = "a", Email = "a@test" },
            new RestaurantUserProfile { Id = userB, UserName = "b", Email = "b@test" });

        var restaurantA = MakeRestaurant(Guid.NewGuid(), "A");
        var restaurantB = MakeRestaurant(Guid.NewGuid(), "B");
        db.Restaurants.AddRange(restaurantA, restaurantB);
        db.Set<RestaurantUser>().AddRange(
            new RestaurantUser { Id = Guid.NewGuid(), UserId = userA, RestaurantId = restaurantA.Id },
            new RestaurantUser { Id = Guid.NewGuid(), UserId = userB, RestaurantId = restaurantB.Id });
        await db.SaveChangesAsync();

        var sut = new RestaurantRepository(db);
        var result = await sut.GetAllAsync(userA);

        Assert.Single(result);
        Assert.Equal(restaurantA.Id, result[0].Id);
    }

    [Fact]
    public async Task FindByIdAsync_WithForeignUserFilter_ReturnsNull()
    {
        await using var db = await CreateDbAsync();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var restaurant = MakeRestaurant(Guid.NewGuid(), "A");

        db.Set<RestaurantUserProfile>().AddRange(
            new RestaurantUserProfile { Id = ownerId, UserName = "owner", Email = "owner@test" },
            new RestaurantUserProfile { Id = otherUserId, UserName = "other", Email = "other@test" });

        db.Restaurants.Add(restaurant);
        db.Set<RestaurantUser>().Add(new RestaurantUser
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            RestaurantId = restaurant.Id
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantRepository(db);
        var result = await sut.FindByIdAsync(restaurant.Id, otherUserId);

        Assert.Null(result);
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

    private static Restaurant MakeRestaurant(Guid id, string name) =>
        new()
        {
            Id = id,
            RestaurantName = name,
            Description = new LangStr(name),
            Summary = new LangStr(name)
        };
}
