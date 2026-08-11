using Base.Domain;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Domain;
using Modules.Restaurants.Infrastructure;
using Moq;
using Shared.Contracts.Restaurants;

namespace WebApp.Tests.Unit.BLL.Active;

public class OpeningHourMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IOpeningHourRepository>();
        var sut = new OpeningHourService(repo.Object);
        var dto = new OpeningHourDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            DayOfWeek = "Monday",
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(18, 0),
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<OpeningHour>(x =>
            x.Id == dto.Id &&
            x.RestaurantId == dto.RestaurantId &&
            x.DayOfWeek!.ToString() == dto.DayOfWeek &&
            x.OpenTime == dto.OpenTime &&
            x.CloseTime == dto.CloseTime &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var domain = new OpeningHour
        {
            Id = id,
            RestaurantId = restaurantId,
            DayOfWeek = new LangStr("Monday"),
            OpenTime = new TimeOnly(9, 0),
            CloseTime = new TimeOnly(18, 0),
            Summary = new LangStr("summary"),
            Restaurant = new Restaurant
            {
                Id = restaurantId,
                RestaurantName = "R Name"
            }
        };

        var repo = new Mock<IOpeningHourRepository>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        var sut = new OpeningHourService(repo.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(restaurantId, dto.RestaurantId);
        Assert.Equal("Monday", dto.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), dto.OpenTime);
        Assert.Equal(new TimeOnly(18, 0), dto.CloseTime);
        Assert.Equal("summary", dto.Summary);
        Assert.NotNull(dto.Restaurant);
        Assert.Equal(restaurantId, dto.Restaurant!.Id);
        Assert.Equal("R Name", dto.Restaurant.RestaurantName);
    }
}
