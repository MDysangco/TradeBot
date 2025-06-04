using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.NewFolder
{
	internal class Base
	{
		public class BaseWallet
		{
			private readonly string _apiKey;
			private readonly HttpClient _httpClient;

			public BaseWallet(string apiKey)
			{
				_apiKey = apiKey;
				_httpClient = new HttpClient();
			}

			public async Task<decimal> GetBaseEthBalanceAsync(string walletAddress)
			{
				string url = $"https://api.basescan.org/api?module=account&action=balance&address={walletAddress}&tag=latest&apikey={_apiKey}";
				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);
				var weiBalance = result["result"].Value<decimal>();
				var ethBalance = weiBalance / 1_000_000_000_000_000_000m; // Convert Wei to ETH
				return ethBalance;
			}

			public async Task GetErc20TokenBalancesAsync(string walletAddress)
			{
				string url = $"https://api.basescan.org/api?module=account&action=tokentx&address={walletAddress}&page=1&offset=100&sort=asc&apikey={_apiKey}";
				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);

				if (result["status"]?.ToString() != "1")
				{
					Console.WriteLine($"Error fetching token transactions: {result["message"]} - {result["result"]}");
					return;
				}

				var tokens = result["result"];
				var tokenBalances = new Dictionary<string, decimal>();

				foreach (var tx in tokens)
				{
					var tokenSymbol = tx["tokenSymbol"]?.ToString();
					var tokenDecimal = int.Parse(tx["tokenDecimal"]?.ToString() ?? "18");
					var value = decimal.Parse(tx["value"]?.ToString() ?? "0") / (decimal)Math.Pow(10, tokenDecimal);
					var toAddress = tx["to"]?.ToString().ToLower();
					var fromAddress = tx["from"]?.ToString().ToLower();
					var contractAddress = tx["contractAddress"]?.ToString().ToLower();

					if (!tokenBalances.ContainsKey(contractAddress))
						tokenBalances[contractAddress] = 0;

					if (toAddress == walletAddress.ToLower())
						tokenBalances[contractAddress] += value;
					else if (fromAddress == walletAddress.ToLower())
						tokenBalances[contractAddress] -= value;
				}

				foreach (var token in tokenBalances)
				{
					if (token.Value > 0)
						Console.WriteLine($"{token.Key}: {token.Value}");
				}
			}

		}
	}
}
