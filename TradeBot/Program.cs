using Nethereum.Web3;
using Nethereum.Util;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static TradeBot.NewFolder.Ethereum;
using static TradeBot.NewFolder.Base;
using TradeBot.Token;
using TradeBot;
using Microsoft.Extensions.Configuration;


internal class Program
{
	private static async Task Main(string[] args)
	{
		// Load appsettings.json
		var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		string walletAddress = config["EthereumWalletAddres"] ?? "";
		string EtherscanApiKey = config["EtherscanApiKey"] ?? "";
		string BasescanApiKey = config["BasescanApiKey"] ?? "";
		string MetaMaskApiKey = config["MetaMaskApiKey"] ?? "";
		string ChatgptApiKey = config["ChatgptApiKey"] ?? "";

		var tokensDictionary = config.GetSection("TokenWalletAddress").Get<Dictionary<string, string>>();
		List<string> TokenList = tokensDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList();



		////METAMASK
		////Used to get ETH balance.
		//string MetaMaskApiKey = "444fb1d16de24a01b405fa4b8e0790e4";

		//var UniswapWallet = new TradeBot.NewFolder.UniSwap.UniswapWallet($"https://mainnet.infura.io/v3/{MetaMaskApiKey}");
		//var accountInfo = await UniswapWallet.GetAccountAsync(walletAddress);

		//Console.WriteLine($"Address: {accountInfo.Address}");
		//Console.WriteLine($"ETH Balance: {accountInfo.EthBalance} ETH");

		////ETHERSCAN
		////Used to get token balance on the ETH chain.
		//string EtherscanApiKey = "X6MMYTJFF5DHA4ZCPB6MWCGATUS32X1U1X";

		//var EtherscanWallet = new EtherscanWallet(EtherscanApiKey);  
		//await EtherscanWallet.GetErc20TokenBalancesAsync(walletAddress);

		////BASESCAN
		////Used to get token balance on the base chain.
		//string BasescanKey = "7T91TRSPG2SVKXB1BYWXAVYWC4PS3IXHK4";

		//var Basewallet = new TradeBot.NewFolder.Ethereum.BaseWallet(BasescanKey);
		//await Basewallet.GetErc20TokenBalancesAsync(walletAddress);


		//DexScreener
		var client = new DexScreenerClient();

		// Example for ETH and Base tokens:
		string landwolf = await client.GetTokenInfo("ethereum", new[] {
			"0x67466BE17df832165F8C80a5A120CCc652bD7E69" // LANDWOLF
		});

		//await client.GetTokenInfo("base", new[] {
		//	"0x4200000000000000000000000000000000000006" // Base USDC or other
		//});


		Chatgpt.ChatGPTAnalyzer chatgpt = new Chatgpt.ChatGPTAnalyzer(ChatgptApiKey);

		//string tokenJson = landwolf;
		//await chatgpt.AnalyzeAsync(tokenJson);
		//string decision = await chatgpt.AnalyzeAsync(tokenJson);
		//Console.WriteLine($"AI decision: {decision}");


		string imagePath = "C:\\Github\\TradeBot\\TradeBot\\Images\\Landwolf.png"; // Replace with your path
		byte[] imageBytes = File.ReadAllBytes(imagePath);
		string base64Image = Convert.ToBase64String(imageBytes);

		string choice = await chatgpt.AnalyzeChartWithImageAsync(base64Image);

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
}