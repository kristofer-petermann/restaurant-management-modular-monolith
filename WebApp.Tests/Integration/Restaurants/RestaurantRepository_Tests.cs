using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Restaurants.Infrastructure;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Integration.Restaurants;

public class RestaurantRepository_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RestaurantRepository_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Seeded_Restaurant_Is_Available_In_RestaurantsDbContext()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantsDbContext>();

        var restaurant = db.Restaurants.SingleOrDefault(x => x.Id == TestUsers.RestaurantAId);
        restaurant.Should().NotBeNull();
        restaurant!.RestaurantName.Should().Be("Restaurant A");
    }
}
