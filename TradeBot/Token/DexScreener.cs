using System.Text;
using System.Text.Json;
using static TradeBot.Network.Ethereum;

namespace TradeBot.Token
{
	internal class DexScreenerClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

		public async Task<DexTokenData?> GetTokenInfo(Helper.Chain chain, string tokenAddresses)
		{
			string joinedAddresses = string.Join(",", tokenAddresses);
			string url = $"https://api.dexscreener.com/tokens/v1/{chain.ToString().ToLower()}/{joinedAddresses}";

			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("Accept", "*/*");

			HttpResponseMessage response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine($"Error: {response.StatusCode}");
				return null;
			}

			string json = await response.Content.ReadAsStringAsync();

			try
			{
				return JsonSerializer.Deserialize<List<DexTokenData>>(json)?.FirstOrDefault();
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"JSON Parse Error: {ex.Message}");
				return null;
			}
		}

		public string BuildExtraPrompt(List<TokenBalance> tokenBalances, DexTokenData data)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Wallet Summary: ");
			sb.AppendLine($"ETH Balance: {tokenBalances.FirstOrDefault(t => t.Symbol?.ToLower() == "eth")?.Balance ?? 0.00m:0.00}");
			sb.AppendLine($"USDT Balance: {tokenBalances.FirstOrDefault(t => t.Symbol?.ToLower() == "usdt")?.Balance ?? 0.00m:0.00}");
			sb.AppendLine($"{data.baseToken.name.ToUpper()} Balance: {tokenBalances.FirstOrDefault(t => t.ContractAddress.ToLower() == data.baseToken.address.ToLower())?.Balance: 0.00m}");

			sb.AppendLine($"\n{data.baseToken.name} Summary: ");
			sb.AppendLine($"Token: {data.baseToken.symbol} ({data.baseToken.name})");
			sb.AppendLine($"Chain: {data.chainId}");
			sb.AppendLine($"DEX: {data.dexId}");
			sb.AppendLine($"URL: {data.url}\n");

			sb.AppendLine($"Current Price: ${data.priceUsd} ({data.priceNative} ETH)");
			sb.AppendLine($"Liquidity: ${data.liquidity.usd:N2}");
			sb.AppendLine($"FDV / Market Cap: ${data.fdv:N0}");

			sb.AppendLine("\nPrice Change:");
			sb.AppendLine($"- 1h: {data.priceChange.h1:+0.##%;-0.##%;0%}");
			sb.AppendLine($"- 6h: {data.priceChange.h6:+0.##%;-0.##%;0%}");
			sb.AppendLine($"- 24h: {data.priceChange.h24:+0.##%;-0.##%;0%}");

			sb.AppendLine("\nTrading Volume:");
			sb.AppendLine($"- 1h: ${data.volume.h1:N2}");
			sb.AppendLine($"- 6h: ${data.volume.h6:N2}");
			sb.AppendLine($"- 24h: ${data.volume.h24:N2}");

			sb.AppendLine("\nBuys/Sells:");
			sb.AppendLine($"- 1h: {data.txns.h1.buys} buys / {data.txns.h1.sells} sells");
			sb.AppendLine($"- 6h: {data.txns.h6.buys} buys / {data.txns.h6.sells} sells");
			sb.AppendLine($"- 24h: {data.txns.h24.buys} buys / {data.txns.h24.sells} sells");

			if (data.info?.websites != null && data.info.websites.Count > 0)
			{
				sb.AppendLine("\nWebsite:");
				foreach (var site in data.info.websites)
					sb.AppendLine($"- {site.url}");
			}

			if (data.info?.socials != null && data.info.socials.Count > 0)
			{
				sb.AppendLine("\nSocials:");
				foreach (var social in data.info.socials)
					sb.AppendLine($"- {social.type}: {social.url}");
			}

			return sb.ToString();
		}

	}

	public class DexScreenerResponse
	{
		public List<DexTokenData> pairs { get; set; }
	}

	public class DexTokenData
	{
		public string chainId { get; set; }
		public string dexId { get; set; }
		public string url { get; set; }
		public string pairAddress { get; set; }
		public Token baseToken { get; set; }
		public Token quoteToken { get; set; }
		public string priceNative { get; set; }
		public string priceUsd { get; set; }
		public Txns txns { get; set; }
		public Volume volume { get; set; }
		public PriceChange priceChange { get; set; }
		public Liquidity liquidity { get; set; }
		public long fdv { get; set; }
		public long marketCap { get; set; }
		public Info info { get; set; }

		public class Token
		{
			public string name { get; set; }
			public string symbol { get; set; }
			public string address { get; set; }
		}

		public class Txns
		{
			public BuysSells m5 { get; set; }
			public BuysSells h1 { get; set; }
			public BuysSells h6 { get; set; }
			public BuysSells h24 { get; set; }
			public class BuysSells
			{
				public int buys { get; set; }
				public int sells { get; set; }
			}
		}

		public class Volume
		{
			public double m5 { get; set; }
			public double h1 { get; set; }
			public double h6 { get; set; }
			public double h24 { get; set; }
		}

		public class PriceChange
		{
			public double h1 { get; set; }
			public double h6 { get; set; }
			public double h24 { get; set; }
		}

		public class Liquidity
		{
			public double usd { get; set; }
			public double baseAmount { get; set; }
			public double quote { get; set; }
		}

		public class Info
		{
			public List<Social> socials { get; set; }
			public List<Website> websites { get; set; }

			public class Social
			{
				public string type { get; set; }
				public string url { get; set; }
			}
			public class Website
			{
				public string label { get; set; }
				public string url { get; set; }
			}
		}
	}




}
