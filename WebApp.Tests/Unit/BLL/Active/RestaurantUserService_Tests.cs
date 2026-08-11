using Base.Domain;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Moq;
using Shared.Contracts.Identity;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class RestaurantUserService_Tests
{
    [Fact]
    public async Task IsRestaurantAdminAsync_RespectsRestaurantScope()
    {
        await using var db = await CreateDbAsync();
        var userId = Guid.NewGuid();
        var restaurantA = Guid.NewGuid();
        var restaurantB = Guid.NewGuid();

        db.Users.Add(new AppUser { Id = userId, UserName = "u", Email = "u@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantA,
            UserRole = EUserRoleInRestaurant.Admin,
            Role = "admin",
            Summary = new LangStr("s")
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        Assert.True(await sut.IsRestaurantAdminAsync(userId));
        Assert.True(await sut.IsRestaurantAdminAsync(userId, restaurantA));
        Assert.False(await sut.IsRestaurantAdminAsync(userId, restaurantB));
    }

    [Fact]
    public async Task GetAllowedRestaurantsAsync_UsesMediatorSummary()
    {
        await using var db = await CreateDbAsync();
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        db.Users.Add(new AppUser { Id = userId, UserName = "u", Email = "u@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            UserRole = EUserRoleInRestaurant.Admin,
            Role = "admin",
            Summary = new LangStr("s")
        });
        await db.SaveChangesAsync();

        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetRestaurantSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantSummaryResponse(restaurantId, "R Name", null));

        var sut = new RestaurantUserService(db, mediator.Object);
        var allowed = await sut.GetAllowedRestaurantsAsync(userId);

        Assert.Single(allowed);
        Assert.Equal(restaurantId, allowed[0].Id);
        Assert.Equal("R Name", allowed[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_BindsUserIdFromAppUserContext()
    {
        await using var db = await CreateDbAsync();
        var ownerUserId = Guid.NewGuid();
        var boundUserId = Guid.NewGuid();
        var rowId = Guid.NewGuid();

        db.Users.AddRange(
            new AppUser { Id = ownerUserId, UserName = "owner", Email = "owner@test" },
            new AppUser { Id = boundUserId, UserName = "bound", Email = "bound@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = rowId,
            UserId = ownerUserId,
            RestaurantId = Guid.NewGuid(),
            UserRole = EUserRoleInRestaurant.Normal,
            Role = "user",
            Summary = new LangStr("s")
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        var dto = await sut.UpdateAsync(new RestaurantUserDto
        {
            Id = rowId,
            UserId = ownerUserId,
            RestaurantId = Guid.NewGuid(),
            UserRole = RestaurantUserRole.Admin,
            Role = "admin",
            Summary = "updated"
        }, boundUserId);

        Assert.Equal(boundUserId, dto.UserId);
        var persisted = await db.RestaurantUsers.FirstAsync(x => x.Id == rowId);
        Assert.Equal(boundUserId, persisted.UserId);
        Assert.Equal(EUserRoleInRestaurant.Admin, persisted.UserRole);
    }

    [Fact]
    public async Task RemoveAsync_WithScopedUser_DoesNotRemoveForeignRow()
    {
        await using var db = await CreateDbAsync();
        var ownerUserId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        var rowId = Guid.NewGuid();

        db.Users.AddRange(
            new AppUser { Id = ownerUserId, UserName = "owner", Email = "owner@test" },
            new AppUser { Id = foreignUserId, UserName = "foreign", Email = "foreign@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = rowId,
            UserId = ownerUserId,
            RestaurantId = Guid.NewGuid(),
            UserRole = EUserRoleInRestaurant.Admin,
            Role = "admin",
            Summary = new LangStr("s")
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        await sut.RemoveAsync(rowId, foreignUserId);

        Assert.NotNull(await db.RestaurantUsers.FirstOrDefaultAsync(x => x.Id == rowId));
    }

    [Fact]
    public async Task FindAsync_WithForeignScope_ReturnsNull()
    {
        await using var db = await CreateDbAsync();
        var ownerUserId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        var rowId = Guid.NewGuid();

        db.Users.AddRange(
            new AppUser { Id = ownerUserId, UserName = "owner", Email = "owner@test" },
            new AppUser { Id = foreignUserId, UserName = "foreign", Email = "foreign@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = rowId,
            UserId = ownerUserId,
            RestaurantId = Guid.NewGuid(),
            UserRole = EUserRoleInRestaurant.Admin,
            Role = "admin",
            Summary = new LangStr("s")
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        var result = await sut.FindAsync(rowId, foreignUserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAdminRestaurantIdsAsync_ReturnsOnlyAdminRows()
    {
        await using var db = await CreateDbAsync();
        var userId = Guid.NewGuid();
        var adminRestaurantId = Guid.NewGuid();
        var normalRestaurantId = Guid.NewGuid();

        db.Users.Add(new AppUser { Id = userId, UserName = "u", Email = "u@test" });
        db.RestaurantUsers.AddRange(
            new RestaurantUser
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RestaurantId = adminRestaurantId,
                UserRole = EUserRoleInRestaurant.Admin,
                Role = "admin",
                Summary = new LangStr("s")
            },
            new RestaurantUser
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RestaurantId = normalRestaurantId,
                UserRole = EUserRoleInRestaurant.Normal,
                Role = "user",
                Summary = new LangStr("s")
            });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        var ids = await sut.GetAdminRestaurantIdsAsync(userId);

        Assert.Single(ids);
        Assert.Equal(adminRestaurantId, ids[0]);
    }

    private static async Task<IdentityDbContext> CreateDbAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new IdentityDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
