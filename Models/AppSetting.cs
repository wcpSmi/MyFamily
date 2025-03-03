using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace MyFamily
{
	internal class AppSetting
	{

		private static string connectionString;
		public static string UserName { get; set; } = Preferences.Get("UserName", string.Empty);
		public static readonly string RedisChannel = "FamilyLocator";
		public static string UUID { get; set; } = Preferences.Get("UUID", string.Empty);

		public static string ConnectionString
		{
			get
			{
				return Preferences.Get("ConnectionString", string.Empty);
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					Preferences.Set("ConnectionString", value.Trim());
					connectionString = value;
				}
			}
		}

		public static void CheckAppSetting(MainPage mainPage)//, RedisService redisService)
		{
			CheckUUID();
			CheckUserName(mainPage);
			CheckConnectionString(mainPage);

		}

		private static void CheckUUID()
		{
			string uuid = Preferences.Get("UUID", string.Empty);
			if (uuid == string.Empty)
			{
				uuid = Guid.NewGuid().ToString();
				Preferences.Set("UUID", uuid);
			}
			UUID = uuid;
		}

		private static void CheckUserName(MainPage mainPage)
		{
			string nev = Preferences.Get("UserName", string.Empty);
			if (nev == string.Empty)
			{

				mainPage.OnMenuClicked(mainPage, EventArgs.Empty);

			}
			nev = Preferences.Get("UserName", string.Empty);
			mainPage.Title = "Family - " + nev;
			UserName = nev;
		}

		private static void CheckConnectionString(MainPage mainPage)
		{
			bool isValid = RedisService.IsConnectSuccess;

			if (!isValid)
			{
				// Ha a kapcsolat érvénytelen, nyissuk meg a beállítási oldalt
				mainPage.OnMenuClicked(mainPage, EventArgs.Empty);
			}
		}

	}
}

