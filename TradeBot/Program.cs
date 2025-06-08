using static TradeBot.Network.Ethereum;
using TradeBot.Token;
using TradeBot;
using Microsoft.Extensions.Configuration;
using TradeBot.Network;
using static TradeBot.Helper;
using System.Text.Json;

internal class Program
{
	private static async Task Main(string[] args)
	{
		// Load appsettings.json
		var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		//Assign keys.
		string walletAddress = config["EthereumWalletAddres"] ?? "";
		string EtherscanApiKey = config["EtherscanApiKey"] ?? "";
		string BasescanApiKey = config["BasescanApiKey"] ?? "";
		string MetaMaskApiKey = config["MetaMaskApiKey"] ?? "";
		string ChatgptApiKey = config["ChatgptApiKey"] ?? "";
		string EthMainnetURl = config["EthMainnetURl"] ?? "";

		var tokensDictionary = config.GetSection("TokenWalletAddress").Get<Dictionary<string, string>>();
		var TokenList = tokensDictionary.Select(kvp => new TokenAddress { Chain = (Chain)int.Parse(kvp.Key), CoinAddress = kvp.Value }).ToList();

		//Calls to ETH mainnet using MetaMaskApi to get wallet ETH balance.
		Phantom.PhantomWallet UniswapWallet = new TradeBot.Network.Phantom.PhantomWallet($"{EthMainnetURl}{MetaMaskApiKey}");
		AccountInfo accountInfo = await UniswapWallet.GetAccountAsync(walletAddress);

		//Intialize Empty token list;
		List<TokenBalance> tokenBalances = new List<TokenBalance>();

		//Calls to Etherscan API to get balance of Erc20 tokens in wallet.
		EtherscanWallet EtherscanWallet = new EtherscanWallet(EtherscanApiKey);
		tokenBalances.AddRange(await EtherscanWallet.GetErc20TokenBalancesAsync(walletAddress));

		//Calls to Basescan API to get balance of Base tokens in wallet.
		BaseWallet Basewallet = new Ethereum.BaseWallet(BasescanApiKey);
		tokenBalances.AddRange(await Basewallet.GetBaseErc20TokenBalancesAsync(walletAddress));

		//Filter the other coins that we're not interested in.
		tokenBalances = tokenBalances.Where(t => tokensDictionary.Any(d => d.Value.ToLower() == t.ContractAddress.ToLower())).ToList();

		//The while loop starts here

		//Call to Dexscreener API to get coin data.
		DexScreenerClient dexScreenerClient = new DexScreenerClient();

		//Initialize chatgpt analyzer.
		Chatgpt.ChatGPTAnalyzer chatgpt = new Chatgpt.ChatGPTAnalyzer(ChatgptApiKey);

		foreach (TokenBalance token in tokenBalances)
		{
			try
			{
				//Get coin data from dex screener
				DexTokenData? coinData = await dexScreenerClient.GetTokenInfo(token.Chain, token.ContractAddress);

				bool CapturedGraph = await TradeView.RunAsync(coinData);

				string choice = await chatgpt.AnalyzeChartWithImageAsync(coinData, dexScreenerClient.BuildExtraPrompt(tokenBalances, coinData));

				switch (choice)
				{
					case "BUY_TOKEN":
						Chatgpt.BuyCoin();
						break;
					case "SELL_TOKEN":
						Chatgpt.SellCoin();
						break;
					case "HOLD_TOKEN":
						Chatgpt.HoldCoin();
						break;
					default:
						Console.WriteLine("No valid decision received.");
						break;
				}

			}
			catch (Exception ex) 
			{ 
				Console.WriteLine(ex);
			}



		}

	}
}