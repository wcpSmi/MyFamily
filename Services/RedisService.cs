using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Channels;

namespace MyFamily
{


	public static class RedisService 
	{
		private static  ConnectionMultiplexer redis;
		private static IDatabase database;
		private static ISubscriber subscriber;
		private static readonly object lockObj = new object();

		public static bool IsConnectSuccess => redis != null && redis.IsConnected;


		static RedisService()
		{
			TryConnect(AppSetting.ConnectionString);
		}

		public static void TryConnect(string conectionString)
		{
			lock (lockObj)
			{
				try
				{
					if (redis != null)
					{
						redis.Dispose(); // Ha van régi kapcsolat, lezárjuk
					}

					redis = ConnectionMultiplexer.Connect(conectionString);
					database = redis.GetDatabase();
					subscriber = redis.GetSubscriber();

					Debug.Print("Redis sikeresen csatlakozott.");
				}
				catch (Exception ex)
				{
					Debug.Print("Redis kapcsolat sikertelen: " + ex.Message);
					redis = null;
				}
			}
		}


		// Üzenetek lekérése a listából
		public static  async Task<List<string>> GetMessagesAsync(string channel, int maxMessages = 100)
		{


			if (!IsConnectSuccess) return new List<string>();

			var messages = await database.ListRangeAsync(channel, -maxMessages, -1);
			return messages.Select(m => m.ToString()).ToList();
		}

		// Üzenet küldése
		public static async Task SendMessageAsync(string channel, Message message, bool store = true)
		{

			if (!IsConnectSuccess)
			{
				Debug.Print("A Redis kapcsolat nem aktív!");
				return;
			}

			if (message is null)
			{
				return;
			}

			var jsonText = MessageHandler.ToJsonText(message);
			Debug.Print("RedisService.SendMessage: " + jsonText.ToString());

			// Üzenet publikálása a csatornára
			await subscriber.PublishAsync(channel, jsonText.ToString());

			// Üzenet tárolása (ha szükséges)
			if (store)
			{
				await database.ListLeftPushAsync(channel, jsonText.ToString());
			}
		}

		// Üzenet fogadása
		private static Action<Message> _messageHandler;
		public static void Subscribe(string channel, Action<Message> messageHandler)
		{

			if (!IsConnectSuccess) return;

			_messageHandler = messageHandler;
			subscriber.Subscribe(channel, (channel, jsonStr) =>
			{
				var message = MessageHandler.JsonStrToMessage(jsonStr);
				_messageHandler?.Invoke(message);
			});
		}

		public static async Task<bool> KeyExistsAsync(string key)
		{
			return IsConnectSuccess && await database.KeyExistsAsync(key);
		}

		public static async Task<bool> TestRedisConnection()
		{
			if (!IsConnectSuccess) return false;

			try
			{
				return await database.PingAsync() != TimeSpan.Zero;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static void Dispose()
		{
			redis?.Dispose();
		}
	}
}


