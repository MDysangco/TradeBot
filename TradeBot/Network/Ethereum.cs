using Nethereum.Signer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Token;

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
				string url = $"https://api.basescan.org/api?module=account&action=tokentx&address={walletAddress}&sort=desc&apikey={_apiKey}";

				var response = await _httpClient.GetStringAsync(url);
				var result = JObject.Parse(response);
				var tokens = result["result"];

				var tokenMap = new Dictionary<string, (string symbol, int decimals, decimal total)>();

				foreach (var tx in tokens)
				{
					var symbol = tx["tokenSymbol"]?.ToString() ?? "UNKNOWN";
					var tokenDecimal = int.TryParse(tx["tokenDecimal"]?.ToString(), out var d) ? d : 18;
					var rawValue = decimal.TryParse(tx["value"]?.ToString(), out var v) ? v : 0;
					var value = rawValue / (decimal)Math.Pow(10, tokenDecimal);

					var tokenContract = tx["contractAddress"]?.ToString();
					var to = tx["to"]?.ToString();
					var from = tx["from"]?.ToString();

					if (string.IsNullOrEmpty(tokenContract)) continue;

					if (!tokenMap.ContainsKey(tokenContract))
						tokenMap[tokenContract] = (symbol, tokenDecimal, 0);

					var entry = tokenMap[tokenContract];

					// Track current balance
					if (to.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
					{
						entry.total += value;
					}
					else if (from.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
					{
						entry.total -= value;

					}

					tokenMap[tokenContract] = entry;
					await Task.Delay(1500);
				}

				// Convert to TokenBalance list
				var tokenBalances = tokenMap
					.Where(kvp => kvp.Value.total > 0)
					.Select(kvp => new TokenBalance
					{
						ContractAddress = kvp.Key,
						Symbol = kvp.Value.symbol,
						Decimals = kvp.Value.decimals,
						Balance = kvp.Value.total,
						Chain = Helper.Chain.BASE,

					})
					.ToList();

				return tokenBalances;
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

				// tokenContract => (symbol, decimals, balance, buyInUsd, buyInTokenAmount)
				var tokenMap = new Dictionary<string, (string symbol, int decimals, decimal balance, string UnixTimestamp)>();

				foreach (var tx in tokens)
				{
					try
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
							tokenMap[tokenContract] = (symbol, tokenDecimal, 0, String.Empty);

						var (sym, dec, currentBalance, time) = tokenMap[tokenContract];

						if (to.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						{
							currentBalance += value;
						}
						else if (from.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
						{
							currentBalance -= value;
						}

						tokenMap[tokenContract] = (sym, dec, currentBalance, time);
					} 
					catch (Exception ex) 
					{ 
						Console.WriteLine(ex.ToString());
					}

					await Task.Delay(1500);
				}

				// Create list of TokenBalance objects
				var tokenBalances = tokenMap
					.Where(kvp => kvp.Value.balance > 0)
					.Select(kvp => new TokenBalance
					{
						ContractAddress = kvp.Key,
						Symbol = kvp.Value.symbol,
						Decimals = kvp.Value.decimals,
						Balance = kvp.Value.balance,
						Chain = Helper.Chain.ETHEREUM,
					})
					.ToList();

				return tokenBalances;
			}

		}

		public class TokenBalance
		{
			public Helper.Chain Chain { get; set; }
			public string ContractAddress { get; set; }
			public string Symbol { get; set; }
			public int Decimals { get; set; }
			public decimal Balance { get; set; }
		}

	}
}
