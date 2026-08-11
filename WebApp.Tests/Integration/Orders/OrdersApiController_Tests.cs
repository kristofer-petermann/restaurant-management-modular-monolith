using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Integration.Orders;

public class OrdersApiController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersApiController_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsSeededOrders()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.GetAsync("/api/Orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<WebApp.ApiContracts.v1.OrderResponse>>();
        items.Should().NotBeNull();
        items!.Should().Contain(x => x.Id == TestUsers.OrderAId);
    }

    [Fact]
    public async Task Post_CreatesOrder()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.PostAsJsonAsync("/api/Orders", new WebApp.ApiContracts.v1.OrderCreateRequest
        {
            UserId = TestUsers.UserAId,
            RestaurantId = TestUsers.RestaurantAId,
            PickupTime = DateTime.UtcNow.AddHours(2),
            Summary = "created in api test",
            Items = new List<WebApp.ApiContracts.v1.SubmitOrderItemRequest>
            {
                new() { MenuItemId = TestUsers.MenuItemAId, Quantity = 1 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostOrderRow_AddsOrderRow()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.PostAsJsonAsync("/api/OrderRows", new WebApp.ApiContracts.v1.OrderRowCreateRequest
        {
            OrderId = TestUsers.OrderAId,
            MenuItemId = TestUsers.MenuItemAId,
            Quantity = 1,
            UnitPrice = 10m,
            Summary = "row"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PutPayment_MarksPaymentPaid()
    {
        var client = await CreateAuthenticatedClientAsync(TestUsers.UserAEmail);
        var response = await client.PutAsJsonAsync($"/api/Payments/{TestUsers.PaymentAId}", new WebApp.ApiContracts.v1.PaymentUpdateRequest
        {
            Id = TestUsers.PaymentAId,
            OrderId = TestUsers.OrderAId,
            Amount = 10m,
            PaymentMethod = "Card",
            PaymentStatus = "Paid",
            PaidAt = DateTime.UtcNow,
            Summary = "paid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var client = _factory.CreateClient();
        var token = await JwtTestHelper.GenerateTokenAsync(_factory.Services, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
