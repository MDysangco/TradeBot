using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TradeBot.Token;

namespace TradeBot
{
	internal class Chatgpt
	{
		public class ChatGPTAnalyzer
		{
			private readonly HttpClient _httpClient = new HttpClient();
			private readonly string _apiKey;

			public ChatGPTAnalyzer(string apiKey)
			{
				_apiKey = apiKey;
				_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
			}

			public async Task<string> AnalyzeChartWithImageAsync(DexTokenData dexTokenData, string extraPrompt = "")
			{
				string base64Image = Helper.ConvertCoinChartoBase64String(dexTokenData);

				var body = new
				{
					model = "gpt-4o",
					messages = new object[]
					{
						new
						{
							role = "system",
							content =
									@"You are a seasoned crypto trading analyst with deep knowledge of on-chain activity, tokenomics, and global financial markets.

									When analyzing tokens, consider:
									- Look at the image (price chart) with liquidity heatmap and give a recommendation.
									- General market sentiment (bearish, bullish, sideways).
									- The current state of Bitcoin and Ethereum and how it affects this coin.	
									- Macro-economic indicators (interest rates, inflation, major crypto news).
									- Likely trader psychology based on token volatility.

									Based on this, respond with one clear recommendation: BUY_TOKEN, SELL_TOKEN, or HOLD_TOKEN.

									Only respond with one of those three exact phrases. Do not include explanations."
						},
						new
						{
							role = "user",
							content = new object[]
							{
								new { type = "text", text = $"Here is the recent data on the coin and my wallet summary. {extraPrompt}" },
								new
								{
									type = "image_url",
									image_url = new
									{
										url = $"data:image/png;base64,{base64Image}"
									}
								}
							}
						}
					}
				};

				var json = JsonSerializer.Serialize(body);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
				var result = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine($"OpenAI API Error: {response.StatusCode} - {result}");
					return "ERROR";
				}

				using var doc = JsonDocument.Parse(result);
				var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
				return message?.Trim().ToUpperInvariant() ?? "ERROR";
			}

		}

		public static void BuyCoin()
		{
			Console.WriteLine("🔥 Triggering BUY...");
			// Add logic to interact with your trading bot
		}

		public static void SellCoin()
		{
			Console.WriteLine("💸 Triggering SELL...");
		}

		public static void HoldCoin()
		{
			Console.WriteLine("🤝 Holding for now...");
		}

	}

	public class ChatRequest
	{
		public string Model { get; set; } = "gpt-4o";
		public List<Message> Messages { get; set; }
	}

	public class Message
	{
		public string Role { get; set; }
		public string Content { get; set; }
	}

	public class ChatResponse
	{
		public List<Choice> Choices { get; set; }
	}

	public class Choice
	{
		public Message Message { get; set; }
	}

}


