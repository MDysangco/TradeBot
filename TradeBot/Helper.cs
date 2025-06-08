using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Token;

namespace TradeBot
{
	internal class Helper
	{
		public class AccountInfo
		{
			public string Address { get; set; }
			public decimal EthBalance { get; set; }
		}

		public class TokenAddress
		{
			public Chain Chain { get; set; }
			public string CoinAddress { get; set; }
		}

		public static string ConvertCoinChartoBase64String(DexTokenData dexTokenData) 
		{
			string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
			string imagePath = Path.Combine(projectDir, "Images", $"{dexTokenData.baseToken.name}-chart.png");
			byte[] imageBytes = File.ReadAllBytes(imagePath);

			return Convert.ToBase64String(imageBytes);
		}

		public enum Chain
		{
			ETHEREUM = 1,
			BASE = 2,
			SOLANA = 3,
		}

	}
}
