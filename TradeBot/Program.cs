using static TradeBot.Network.Ethereum;
using TradeBot.Token;
using TradeBot;
using Microsoft.Extensions.Configuration;
using TradeBot.Network;
using static TradeBot.Helper;


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
		var TokenList = tokensDictionary.Select(kvp => new TokenAddress { Chain = (DexScreenerClient.Chain)int.Parse(kvp.Key), CoinAddress = kvp.Value }).ToList();

		//Calls to ETH mainnet using MetaMaskApi to get wallet ETH balance.
		Phantom.PhantomWallet UniswapWallet = new TradeBot.Network.Phantom.PhantomWallet($"{EthMainnetURl}{MetaMaskApiKey}");
		Helper.AccountInfo accountInfo = await UniswapWallet.GetAccountAsync(walletAddress);

		Console.WriteLine($"ETH Balance: {accountInfo.EthBalance} ETH");

		//Intialize Empty token list;
		List<TokenBalance> tokenBalances = new List<TokenBalance>();

		//Calls to Etherscan API to get balance of Erc20 tokens in wallet.
		EtherscanWallet EtherscanWallet = new EtherscanWallet(EtherscanApiKey);
		tokenBalances.AddRange(await EtherscanWallet.GetErc20TokenBalancesAsync(walletAddress));

		//Calls to Basescan API to get balance of Base tokens in wallet.
		BaseWallet Basewallet = new Ethereum.BaseWallet(BasescanApiKey);
		tokenBalances.AddRange(await Basewallet.GetBaseErc20TokenBalancesAsync(walletAddress));

		//Call to Dexscreener API to get coin data.
		DexScreenerClient dexScreenerClient = new DexScreenerClient();

		foreach (TokenAddress token in TokenList)
		{
			string coindData = await dexScreenerClient.GetTokenInfo(token.Chain, token.CoinAddress);
		}


		//Chatgpt.ChatGPTAnalyzer chatgpt = new Chatgpt.ChatGPTAnalyzer(ChatgptApiKey);

		////string tokenJson = landwolf;
		////await chatgpt.AnalyzeAsync(tokenJson);
		////string decision = await chatgpt.AnalyzeAsync(tokenJson);
		////Console.WriteLine($"AI decision: {decision}");

		//string imagePath = "C:\\Github\\TradeBot\\TradeBot\\Images\\Landwolf.png"; // Replace with your path
		//byte[] imageBytes = File.ReadAllBytes(imagePath);
		//string base64Image = Convert.ToBase64String(imageBytes);

		//string choice = await chatgpt.AnalyzeChartWithImageAsync(base64Image);

		//switch (choice)
		//{
		//	case "BUY_TOKEN":
		//		Chatgpt.BuyCoin();
		//		break;
		//	case "SELL_TOKEN":
		//		Chatgpt.SellCoin();
		//		break;
		//	case "HOLD_TOKEN":
		//		Chatgpt.HoldCoin();
		//		break;
		//	default:
		//		Console.WriteLine("No valid decision received.");
		//		break;
		//}
	}
}