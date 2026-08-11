using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Domain;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.Integration.Identity;

public class IdentitySeed_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IdentitySeed_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Seed_Creates_Test_Users()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        (await userManager.FindByEmailAsync(TestUsers.UserAEmail)).Should().NotBeNull();
        (await userManager.FindByEmailAsync(TestUsers.UserBEmail)).Should().NotBeNull();
    }
}
