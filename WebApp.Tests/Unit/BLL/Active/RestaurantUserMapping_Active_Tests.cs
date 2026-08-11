using Base.Domain;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;
using Moq;
using Shared.Contracts.Identity;

namespace WebApp.Tests.Unit.BLL.Active;

public class RestaurantUserMapping_Active_Tests
{
    [Fact]
    public async Task Add_MapsDtoToDomain_AndBindsUser()
    {
        await using var db = await CreateDbAsync();
        var boundUserId = Guid.NewGuid();
        db.Users.Add(new AppUser { Id = boundUserId, UserName = "u", Email = "u@test" });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        sut.Add(new RestaurantUserDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            UserRole = RestaurantUserRole.Admin,
            Role = "admin",
            Summary = "summary"
        }, boundUserId);
        await db.SaveChangesAsync();

        var row = await db.RestaurantUsers.FirstAsync();
        Assert.Equal(boundUserId, row.UserId);
        Assert.Equal(EUserRoleInRestaurant.Admin, row.UserRole);
        Assert.Equal("summary", row.Summary!.ToString());
    }

    [Fact]
    public async Task FindEntityAsync_MapsDomainToDto_AllFieldsCopied()
    {
        await using var db = await CreateDbAsync();
        var userId = Guid.NewGuid();
        var rowId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        db.Users.Add(new AppUser { Id = userId, UserName = "u", Email = "u@test" });
        db.RestaurantUsers.Add(new RestaurantUser
        {
            Id = rowId,
            UserId = userId,
            RestaurantId = restaurantId,
            UserRole = EUserRoleInRestaurant.Admin,
            Role = "admin",
            Summary = new LangStr("summary")
        });
        await db.SaveChangesAsync();

        var sut = new RestaurantUserService(db, new Mock<IMediator>().Object);
        var dto = await sut.FindEntityAsync(rowId);

        Assert.NotNull(dto);
        Assert.Equal(rowId, dto!.Id);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.Equal(RestaurantUserRole.Admin, dto.UserRole);
        Assert.Equal("admin", dto.Role);
        Assert.Equal("summary", dto.Summary);
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
