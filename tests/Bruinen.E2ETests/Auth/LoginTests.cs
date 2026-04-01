namespace Bruinen.E2ETests.Auth;

[Collection(E2ECollection.Name)]
public class LoginTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToHomePage()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync($"{E2ESettings.BaseUrl}/Auth/Login");

        await page.FillAsync("input[name='Username']", E2ESettings.TestUsername);
        await page.FillAsync("input[name='Password']", E2ESettings.TestPassword);
        await page.ClickAsync("button[type='submit']");

        await page.WaitForURLAsync(url => !url.Contains("/Auth/Login"));

        Assert.Equal("Home Page - Bruinen", await page.TitleAsync());
    }
}
