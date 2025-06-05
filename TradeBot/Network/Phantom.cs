using Nethereum.Util;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Network
{
	internal class Phantom
	{
		public class PhantomWallet
		{
			private readonly Web3 web3;

			public PhantomWallet(string rpcUrl)
			{
				web3 = new Web3(rpcUrl);
			}

			public async Task<Helper.AccountInfo> GetAccountAsync(string walletAddress)
			{
				if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(walletAddress))
				{
					throw new ArgumentException("Invalid wallet address");
				}

				var balanceWei = await web3.Eth.GetBalance.SendRequestAsync(walletAddress);
				var balanceEth = Web3.Convert.FromWei(balanceWei);

				return new Helper.AccountInfo
				{
					Address = walletAddress,
					EthBalance = balanceEth
				};
			}
		}

	}
}
