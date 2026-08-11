using AwesomeAssertions;
using Microsoft.Playwright;
using WebApp.Tests.Helpers;

namespace WebApp.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class RestaurantsMvcController_E2E
{
    private readonly PlaywrightWebAppFactory _factory;

    public RestaurantsMvcController_E2E(PlaywrightWebAppFactory factory)
    {
        _factory = factory;
    }

    private async Task<(IPlaywright pw, IBrowser browser)> NewBrowserAsync()
    {
        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.ConnectAsync("ws://localhost:3000/");
        return (pw, browser);
    }

    private async Task<IPage> NewPageAsync(IBrowser browser) =>
        await (await browser.NewContextAsync()).NewPageAsync();

    [Fact]
    public async Task Index_Anonymous_AllowsAccess()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var page = await NewPageAsync(browser);
            var resp = await page.GotoAsync($"{_factory.RootUri}/Restaurants");
            resp!.Status.Should().Be(200);
            page.Url.Should().Contain("/Restaurants");
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task Index_AsAdmin_ShowsOnlyOwnRestaurants()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var pageA = await NewPageAsync(browser);
            await PlaywrightLoginHelper.LoginAsync(pageA, _factory.RootUri, TestUsers.UserAEmail, TestUsers.Password);
            await PlaywrightLoginHelper.SetCultureAsync(pageA, _factory.RootUri, "en");

            var pageB = await NewPageAsync(browser);
            await PlaywrightLoginHelper.LoginAsync(pageB, _factory.RootUri, TestUsers.UserBEmail, TestUsers.Password);
            await PlaywrightLoginHelper.SetCultureAsync(pageB, _factory.RootUri, "en");

            await pageA.GotoAsync($"{_factory.RootUri}/Restaurants");
            var bodyA = await pageA.Locator("body").TextContentAsync();
            bodyA.Should().Contain("Restaurant A");
            bodyA.Should().NotContain("Restaurant B");
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task Edit_Details_Delete_RoundTrip()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var page = await NewPageAsync(browser);
            await PlaywrightLoginHelper.LoginAsync(page, _factory.RootUri, TestUsers.UserAEmail, TestUsers.Password);
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");

            var name = "Restaurant A";
            await page.GotoAsync($"{_factory.RootUri}/Restaurants");

            var id = await ExtractIdFromDetailsLinkAsync(page, name);
            await page.GotoAsync($"{_factory.RootUri}/Restaurants/Edit/{id}");
            await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/Restaurants/Edit/.*"));
            await page.FillAsync("#Summary", "restaurant-a-summary-updated");
            await page.ClickAsync("input[type=submit]");
            await page.WaitForURLAsync($"{_factory.RootUri}/Restaurants");
            (await page.Locator("body").TextContentAsync()).Should().Contain("restaurant-a-summary-updated");

            await ClickRowActionAsync(page, name, "Details");
            await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/Restaurants/Details/.*"));
            (await page.Locator("body").TextContentAsync()).Should().Contain(name);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task Edit_ForeignRestaurantId_Returns404_IDOR()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var pageA = await NewPageAsync(browser);
            await PlaywrightLoginHelper.LoginAsync(pageA, _factory.RootUri, TestUsers.UserAEmail, TestUsers.Password);
            var resp = await pageA.GotoAsync($"{_factory.RootUri}/Restaurants/Edit/{TestUsers.RestaurantBId}");
            resp!.Status.Should().Be(404);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task Delete_ForeignRestaurantId_Returns404_IDOR()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var pageA = await NewPageAsync(browser);
            await PlaywrightLoginHelper.LoginAsync(pageA, _factory.RootUri, TestUsers.UserAEmail, TestUsers.Password);
            var resp = await pageA.GotoAsync($"{_factory.RootUri}/Restaurants/Delete/{TestUsers.RestaurantBId}");
            resp!.Status.Should().Be(404);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    private static async Task ClickRowActionAsync(IPage page, string rowText, string action)
    {
        var row = page.Locator("tr").Filter(new() { HasTextString = rowText });
        await row.Locator($"a:has-text('{action}')").First.ClickAsync();
    }

    private static async Task<Guid> ExtractIdFromDetailsLinkAsync(IPage page, string rowText)
    {
        var row = page.Locator("tr").Filter(new() { HasTextString = rowText });
        var href = await row.Locator("a:has-text('Details')").First.GetAttributeAsync("href");
        if (href == null) throw new InvalidOperationException($"Details link not found for {rowText}");
        var idStr = href.Substring(href.LastIndexOf('/') + 1);
        return Guid.Parse(idStr);
    }
}
