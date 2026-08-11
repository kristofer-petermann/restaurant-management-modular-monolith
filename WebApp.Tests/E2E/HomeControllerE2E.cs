using Microsoft.Playwright;

namespace WebApp.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class HomeControllerE2E
{
    private readonly PlaywrightWebAppFactory _factory;

    public HomeControllerE2E(PlaywrightWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task User_CanRegisterAndLogin()
    {
        var email = $"e2e+{Guid.NewGuid():N}@test.ee";
        const string password = "Test.pass1";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.ConnectAsync("ws://localhost:3000/");
        var page = await browser.NewPageAsync();

        // Navigate to register page
        await page.GotoAsync($"{_factory.RootUri}/Identity/Account/Register");

        // Fill out registration form
        await page.FillAsync("#Input_Email", email);
        await page.FillAsync("#Input_Password", password);
        await page.FillAsync("#Input_ConfirmPassword", password);
        await page.ClickAsync("#registerSubmit");

        // Assert — should redirect to home page after registration
        await Assertions.Expect(page).ToHaveURLAsync($"{_factory.RootUri}/");

        // Default Identity UI auto-signs-in after register, so log out before
        // exercising the login flow.
        await page.ClickAsync("form[action*='Logout'] button[type=submit]");
        await Assertions.Expect(page).ToHaveURLAsync($"{_factory.RootUri}/");

        // Navigate to login
        await page.GotoAsync($"{_factory.RootUri}/Identity/Account/Login");
        await page.FillAsync("#Input_Email", email);
        await page.FillAsync("#Input_Password", password);
        await page.ClickAsync("#login-submit");

        // Assert — user is logged in, greeting visible in the nav
        await Assertions.Expect(page.Locator($"text={email}")).ToBeVisibleAsync();
    }
}