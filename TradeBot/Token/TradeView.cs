using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Token
{
	public class TradeView
	{
		public static async Task<bool> RunAsync(DexTokenData tokenData)
		{
			var playwright = await Playwright.CreateAsync();
			var browser = await playwright.Chromium.ConnectOverCDPAsync("http://localhost:9222");
			var context = browser.Contexts.FirstOrDefault();

			if (context == null)
			{
				Console.WriteLine("No browser context found.");
				return false;
			}

			string tokenSymbol = tokenData.baseToken.symbol.ToLower();
			string tokenName = tokenData.baseToken.name.ToLower();

			// Find correct TradingView tab by inspecting page content
			IPage matchedPage = null;
			foreach (var page in context.Pages.Where(p => p.Url.Contains("tradingview.com")))
			{
				try
				{
					string bodyText = await page.EvaluateAsync<string>("() => document.body.innerText.toLowerCase()");
					if (bodyText.Contains(tokenSymbol) || bodyText.Contains(tokenName))
					{
						matchedPage = page;
						break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error reading page content: {ex.Message}");
				}
			}

			if (matchedPage == null)
			{
				Console.WriteLine($"No TradingView tab found containing {tokenSymbol} or {tokenName} in content.");
				return false;
			}

			// Trigger screenshot download via keyboard shortcut
			var downloadTask = matchedPage.RunAndWaitForDownloadAsync(async () =>
			{
				await matchedPage.Keyboard.PressAsync("Control+Alt+S");
			});

			var download = await downloadTask;

			// Save downloaded file
			string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
			string imagesPath = Path.Combine(projectDir, "Images");
			Directory.CreateDirectory(imagesPath);

			// Delete old chart files
			foreach (var file in Directory.GetFiles(imagesPath, $"{tokenData.baseToken.name}-*.png"))
			{
				try { File.Delete(file); }
				catch (Exception ex) { Console.WriteLine($"Failed to delete {file}: {ex.Message}"); }
			}

			string filename = $"{tokenData.baseToken.name}-chart.png";
			string fullPath = Path.Combine(imagesPath, filename);
			await download.SaveAsAsync(fullPath);

			Console.WriteLine($"Chart saved to: {fullPath}");
			return true;
		}

	}
}
