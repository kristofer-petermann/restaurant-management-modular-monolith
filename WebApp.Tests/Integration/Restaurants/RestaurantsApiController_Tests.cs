using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Integration.Restaurants;

public class RestaurantsApiController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RestaurantsApiController_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsSeededRestaurants()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.GetAsync("/api/Restaurants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<WebApp.ApiContracts.v1.RestaurantResponse>>();
        items.Should().NotBeNull();
        items!.Should().Contain(x => x.Id == TestUsers.RestaurantAId);
    }

    [Fact]
    public async Task Post_CreatesRestaurant()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.PostAsJsonAsync("/api/Restaurants", new WebApp.ApiContracts.v1.RestaurantCreateRequest
        {
            RestaurantName = "Created from api test",
            Description = "desc",
            Summary = "summary"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateClient();
        var token = await JwtTestHelper.GenerateTokenAsync(_factory.Services, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
