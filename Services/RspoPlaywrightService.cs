using Microsoft.Playwright;

namespace mapa_back.Services
{
	public class RspoPlaywrightService
	{
		public async Task<string> GetPlacowkiAsync(int rspos = 1)
		{
			using var playwright = await Playwright.CreateAsync();

			var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
			{
				Headless = true
			});

			var context = await browser.NewContextAsync(new BrowserNewContextOptions
			{
				Locale = "pl-PL"
			});

			var page = await context.NewPageAsync();

			string? jsonResponse = null;

			page.Response += async (_, response) =>
			{
				if (response.Url.Contains("/api/placowki"))
				{
					try
					{
						jsonResponse = await response.TextAsync();
					}
					catch { }
				}
			};

			await page.GotoAsync("https://api-rspo.men.gov.pl/", new PageGotoOptions
			{
				WaitUntil = WaitUntilState.NetworkIdle
			});

			await page.GotoAsync($"https://api-rspo.men.gov.pl/api/placowki/?RSPO={rspos}",
				new PageGotoOptions
				{
					WaitUntil = WaitUntilState.NetworkIdle
				});

			await browser.CloseAsync();

			if (jsonResponse == null)
				throw new Exception("Nie udało się pobrać danych z RSPO");

			return jsonResponse;
		}
	}
}