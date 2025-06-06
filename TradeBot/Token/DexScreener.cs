using Microsoft.Playwright;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TradeBot.Token
{
    internal class DexScreenerClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<string> GetTokenInfo(Chain chain, string tokenAddresses)
        {
            string joinedAddresses = string.Join(",", tokenAddresses);
            string url = $"https://api.dexscreener.com/tokens/v1/{chain.ToString().ToLower()}/{joinedAddresses}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "*/*");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return String.Empty;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task RunAsync()
        {
            var playwright = await Playwright.CreateAsync();

            // Connect to already open Edge browser
            var browser = await playwright.Chromium.ConnectOverCDPAsync("http://localhost:9222");
            var context = browser.Contexts.FirstOrDefault();
            var page = context?.Pages.FirstOrDefault(p => p.Url.Contains("tradingview.com"));

            if (page == null)
            {
                Console.WriteLine("❌ Dexscreener tab not found.");
                return;
            }

            Console.WriteLine($"✅ Found Dexscreener tab: {page.Url}");

            //// Wait for the TradingView iframe
            //var tradingViewFrame = page.Frame("tradingview_d998e");
            //if (tradingViewFrame == null)
            //{
            //    Console.WriteLine("❌ Could not find the tradingview iframe.");
            //    return;
            //}

            //// Focus inside the iframe (e.g., body or chart container)
            //await tradingViewFrame.FocusAsync("body");

            // Setup download listener
            var downloadTask = page.RunAndWaitForDownloadAsync(async () =>
            {
                // Trigger the chart screenshot shortcut
                await page.Keyboard.PressAsync("Control+Alt+S");
            });

            var download = await downloadTask;

            string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
            string imagesPath = Path.Combine(projectDir, "Images");
            Directory.CreateDirectory(imagesPath);

            string filename = $"chart-{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(imagesPath, filename);

            await download.SaveAsAsync(fullPath);

            Console.WriteLine($"✅ Chart image saved to: {fullPath}");
        }
        public enum Chain
        {
            ETHEREUM = 1,
            BASE = 2,
            SOLANA = 3,
        }

    }




}



//chrome.exe --remote-debugging-port=9222 --user-data-dir="C:\chrome-profile"
