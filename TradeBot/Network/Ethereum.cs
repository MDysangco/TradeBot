using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.NewFolder
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

			public async Task GetErc20TokenBalancesAsync(string walletAddress)
			{
				string url = $"https://api.etherscan.io/api?module=account&action=tokentx&address={walletAddress}&sort=desc&apikey={_apiKey}";

				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);
				var tokens = result["result"];

				var tokenMap = new Dictionary<string, (string symbol, int decimals, decimal total)>();

				foreach (var tx in tokens)
				{
					var symbol = tx["tokenSymbol"]?.ToString() ?? "UNKNOWN";
					var tokenName = tx["tokenName"]?.ToString() ?? "Unnamed Token";
					var tokenDecimal = int.Parse(tx["tokenDecimal"].ToString());
					var value = decimal.Parse(tx["value"].ToString()) / (decimal)Math.Pow(10, tokenDecimal);
					var tokenContract = tx["contractAddress"].ToString();
					var to = tx["to"].ToString();
					var from = tx["from"].ToString();

					// Approximate balance by aggregating inflow - outflow
					if (!tokenMap.ContainsKey(tokenContract))
						tokenMap[tokenContract] = (symbol, tokenDecimal, 0);

					var balance = tokenMap[tokenContract].total;

					if (to.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						balance += value;
					else if (from.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						balance -= value;

					tokenMap[tokenContract] = (symbol, tokenDecimal, balance);
				}

				foreach (var (contract, (symbol, decimals, balance)) in tokenMap)
				{
					if (balance > 0)
						Console.WriteLine($"{symbol}: {balance:N4}");
				}
			}



		}
	}
}
