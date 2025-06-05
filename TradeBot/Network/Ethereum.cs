using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Network
{
	internal class Ethereum
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

			public async Task<List<TokenBalance>> GetBaseErc20TokenBalancesAsync(string walletAddress)
			{
				string url = $"https://api.basescan.org/api?module=account&action=tokentx&address={walletAddress}&page=1&offset=100&sort=asc&apikey={_apiKey}";
				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);

				if (result["status"]?.ToString() != "1")
				{
					Console.WriteLine($"Error fetching token transactions: {result["message"]} - {result["result"]}");
					return new List<TokenBalance>();
				}

				var tokens = result["result"];
				var tokenMap = new Dictionary<string, (string symbol, int decimals, decimal total)>();

				foreach (var tx in tokens)
				{
					var symbol = tx["tokenSymbol"]?.ToString() ?? "UNKNOWN";
					var tokenDecimal = int.TryParse(tx["tokenDecimal"]?.ToString(), out var d) ? d : 18;
					var value = decimal.TryParse(tx["value"]?.ToString(), out var v) ? v : 0;
					value /= (decimal)Math.Pow(10, tokenDecimal);

					var to = tx["to"]?.ToString();
					var from = tx["from"]?.ToString();
					var contract = tx["contractAddress"]?.ToString()?.ToLower();

					if (string.IsNullOrEmpty(contract)) continue;

					if (!tokenMap.ContainsKey(contract))
						tokenMap[contract] = (symbol, tokenDecimal, 0);

					var balance = tokenMap[contract].total;

					if (to?.Equals(walletAddress, StringComparison.OrdinalIgnoreCase) == true)
						balance += value;
					else if (from?.Equals(walletAddress, StringComparison.OrdinalIgnoreCase) == true)
						balance -= value;

					tokenMap[contract] = (symbol, tokenDecimal, balance);
				}

				return tokenMap
					.Where(kvp => kvp.Value.total > 0)
					.Select(kvp => new TokenBalance
					{
						ContractAddress = kvp.Key,
						Symbol = kvp.Value.symbol,
						Decimals = kvp.Value.decimals,
						Balance = kvp.Value.total
					})
					.ToList();
			}
		}

		public class EtherscanWallet
		{
			private readonly string _apiKey;
			private readonly HttpClient _httpClient;

			public EtherscanWallet(string apiKey)
			{
				_apiKey = apiKey;
				_httpClient = new HttpClient();
			}

			public async Task<decimal> GetEthBalanceAsync(string walletAddress)
			{
				string url = $"https://api.etherscan.io/api?module=account&action=balance&address={walletAddress}&tag=latest&apikey={_apiKey}";

				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);
				var weiBalance = result["result"].Value<decimal>();
				var ethBalance = weiBalance / 1_000_000_000_000_000_000m; // Convert Wei to ETH

				return ethBalance;
			}

			public async Task<List<TokenBalance>> GetErc20TokenBalancesAsync(string walletAddress)
			{
				string url = $"https://api.etherscan.io/api?module=account&action=tokentx&address={walletAddress}&sort=desc&apikey={_apiKey}";

				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);
				var tokens = result["result"];

				var tokenMap = new Dictionary<string, (string symbol, int decimals, decimal total)>();

				foreach (var tx in tokens)
				{
					var symbol = tx["tokenSymbol"]?.ToString() ?? "UNKNOWN";
					var tokenDecimal = int.TryParse(tx["tokenDecimal"]?.ToString(), out var d) ? d : 18;
					var value = decimal.TryParse(tx["value"]?.ToString(), out var v) ? v : 0;
					value /= (decimal)Math.Pow(10, tokenDecimal);

					var tokenContract = tx["contractAddress"]?.ToString();
					var to = tx["to"]?.ToString();
					var from = tx["from"]?.ToString();

					if (string.IsNullOrEmpty(tokenContract)) continue;

					if (!tokenMap.ContainsKey(tokenContract))
						tokenMap[tokenContract] = (symbol, tokenDecimal, 0);

					var balance = tokenMap[tokenContract].total;

					if (to.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						balance += value;
					else if (from.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						balance -= value;

					tokenMap[tokenContract] = (symbol, tokenDecimal, balance);
				}

				// Create list of TokenBalance objects
				var tokenBalances = tokenMap
					.Where(kvp => kvp.Value.total > 0)
					.Select(kvp => new TokenBalance
					{
						ContractAddress = kvp.Key,
						Symbol = kvp.Value.symbol,
						Decimals = kvp.Value.decimals,
						Balance = kvp.Value.total
					})
					.ToList();

				return tokenBalances;
			}

		}

		public class TokenBalance
		{
			public string ContractAddress { get; set; }
			public string Symbol { get; set; }
			public int Decimals { get; set; }
			public decimal Balance { get; set; }
		}

	}
}
