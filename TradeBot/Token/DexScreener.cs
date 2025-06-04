using Nethereum.Contracts;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Token
{
	internal class DexScreenerClient
	{
		private readonly HttpClient _httpClient = new HttpClient();

		public async Task<string> GetTokenInfo(string chainId, string[] tokenAddresses)
		{
			string joinedAddresses = string.Join(",", tokenAddresses);
			string url = $"https://api.dexscreener.com/tokens/v1/{chainId}/{joinedAddresses}";

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


	}
}
