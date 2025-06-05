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
			public DexScreenerClient.Chain Chain { get; set; }
			public string CoinAddress { get; set; }
		}

	}
}
