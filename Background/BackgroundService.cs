#if ANDROID
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using System.Threading.Tasks;
using static Android.Icu.Text.CaseMap;


namespace MyFamily
{

	[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync | global::Android.Content.PM.ForegroundService.TypeRemoteMessaging)]
	public class BackgroundService : Service
	{
		
		static readonly object locker = new object();


		public override void OnCreate()
		{
			base.OnCreate();

			AppSetting.ConnectionString = Preferences.Get("ConnectionString", string.Empty);

			if (string.IsNullOrEmpty(AppSetting.ConnectionString))// Ha nincs Connection String, állítsuk le a szolgáltatást
			{
				StopSelf(); 
				return;
			}

			// Ellenőrizzük, hogy működik-e a kapcsolat**
			if (!RedisService.IsConnectSuccess)// A Redis kapcsolat nem működik, leállítjuk a szolgáltatást.
			{			
				StopSelf();
				return;
			}

			//Android 8.0 (API 26) felett a háttérszolgáltatások előtérszolgáltatásként kell fussanak, ezért hívd meg a StartForeground metódust, hogy stabilan fusson a háttérben.
			StartForegroundService(); // Előtérszolgáltatás indítása


			// Üzenet figyelés a Redis-en
		
			RedisService.Subscribe(AppSetting.RedisChannel, message =>
			{

				// Üzenet feldolgozása, szűrés logika
				if (MessageHandler.IsLocationQuestion(message))//Ha location kérdés
				{
					MyFamily.LocationService.StartService(this); // A hely szolgáltatás indítása
																 // Ha a szolgáltatás nem fut, indítjuk el
					if (!LocationServiceState.IsServiceRunning)
					{
						// Küldj egy broadcastot a LocationService elindítására
						var intent = new Intent("com.wcp.myfamily.LOCATION_REQUEST");
						Android.App.Application.Context.SendBroadcast(intent);
					}

					//var location = LocationHandler.StartLocationUpdates();               

					Task.Run(async () =>
					{
						var locationData = await LocationHandler.LocationData();

						// Küldjük vissza a válasz üzenetet
						Message valaszUzenet = new Message()
						{
							SenderName = AppSetting.UserName,
							SenderUUID = AppSetting.UUID,
							Text = locationData.Item1,
							Date = DateTime.Now.ToString(),
							Location = locationData.Item2,
							CanSwipe =true,
							Host = message.Host
						};

						//TODO Fejlesztés:  a korábbi helyadatok lekérdezése
						//ha le akarod tárolni a üzenetet (helyadatokat) akkor itt a store tulajdonságot true-ra kell állítani
						RedisService.SendMessageAsync(AppSetting.RedisChannel, valaszUzenet, true);

						
						//MyFamily.LocationService.StopService(this);
					});
					NotificationHelper.SimpleNotification(this, message.SenderName, "Helyadatot kért", "Message Channel", true, false);

					
				}
				else //Ha csak üzenet érkezett és nem helyadatot kért le
				{
					//Helyadat üzenet notification
					var myUuid = AppSetting.UUID;
					if (message.Host == myUuid && !MessageHandler.IsSOS(message))
					{
						NotificationHelper.SimpleNotification(this, message.SenderName, message.Text, "Message Channel", true, false);
					}

					if (message.Host == myUuid && MessageHandler.IsSOS(message))
					{
						NotificationHelper.AlarmNotification(this, message.SenderName, message.Text, "ALarm Channel", false, false);
					}

					//Üzenet továbbítása a MainPage-nek 
						ForwardMessageToMainPage(message);

				}
			});
		}



		// Az üzenet továbbítása a MainPage számára
		private void ForwardMessageToMainPage(Message message)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				// A MainPage statikus példányán keresztül meghívjuk az üzenetkezelést
				MainPage.Instance?.HandleReceivedMessage(message);
			});
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			return StartCommandResult.Sticky;
		}

		public static void StartService(Context context)
		{

			var intent = new Intent(context, typeof(BackgroundService));

			// Ha Android 8.0 vagy újabb, indítjuk előtérszolgáltatásként
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				context.StartForegroundService(intent);  // Android 8.0 vagy újabb verziókhoz
			}
			else
			{
				context.StartService(intent);  // Android 7.1 vagy régebbi verziókhoz
			}
		}

		private void StartForegroundService()
		{

			var channelId = "background_service_channel"; 

			var notification = new NotificationCompat.Builder(this, channelId)
				.SetSmallIcon(Resource.Drawable.notifi_icon_bw)
				.SetPriority(NotificationCompat.PriorityLow)
				.SetOngoing(true)
				.SetContentText("Background service is running")
				.SetOnlyAlertOnce(true)
				.SetAutoCancel(true)
				.Build();

			// A szolgáltatás elindítása előtérszolgáltatásként
			StartForeground(1, notification);
		}

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}
	}
}
#endif



