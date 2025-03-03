#if ANDROID
using Android.App;                  // A NotificationChannel és a NotificationManager
using Android.Content;              // A Context osztály
using AndroidX.Core.App;            // A NotificationCompat
using Android.OS;
using Android.Media;                   // A Build.Version és a BuildVersionCodes

namespace MyFamily
{
    public static class NotificationHelper
    {
        /*Főbb Paraméterek
             .SetContentTitle(title)    // Az értesítés címe
            .SetContentText(newMessage)   // Az értesítés üzenete
            .SetSmallIcon(Resource.Drawable.dotnet_bot)  // Ellenőrizd, hogy létezik ez az ikon
            .SetOngoing(true)           //Az értesítés állandó, nem lehet eltüntetni.
            .SetAutoCancel(true)        // Az értesítés eltűnik, ha a felhasználó rákattint.
            .SetContentIntent(pendingIntent)    // Az értesítésre kattintva elindít egy aktivitást.
            .SetPriority(NotificationCompat.PriorityHigh)       // Magas prioritású értesítések(azonosítja, hogy megjelenik-e az értesítési sávban, ha nem a foregroundban vagy).
            .SetSound()         //Hang beállítása.
            .SetVibrate()       // Rezgés beállítása.*/


        private static string CreateChannelID(string channelName)
        {
            return channelName.ToLower().Replace(" ", "_").Trim();
        }



        // <summary>
        /// Megjelenít egy egyszerű értesítést, különböző testreszabási lehetőségekkel.
        /// </summary>
        /// <param name="context">Az alkalmazás kontextusa.</param>
        /// <param name="title">Az értesítés címe.</param>
        /// <param name="newMessage">Az értesítés szövege.</param>
        /// <param name="channelName">Az értesítési csatorna neve, amelyet az Android megjelenít a beállításokban.</param>
        /// <param name="enableKill">Ha <c>true</c>, az értesítés kattintásra automatikusan eltűnik.</param>
        /// <param name="overwrite">Ha <c>true</c>, az értesítés azonos ID-val jelenik meg, felülírva a korábbi azonosítóval rendelkező értesítést.</param>
        public static int SimpleNotification(Context context, string title, string message, string channelName, bool enableKill, bool overwrite)
        {
            int notificationId =-1;

            //Ha az alkalmazás előtérben van akkor kilép
            if (IsAppInForeground(context))
            {
                return notificationId;
            }
            //Activálás beállítása
            var intent = new Intent(context, typeof(MainActivity)); // A fő aktivitásod
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop); // A már meglévő aktivitás újrahasználása
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            var channelId = CreateChannelID(channelName);


            // Notification létrehozása
            var notification = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)    // Az értesítés címe
                .SetContentText(message)   // Az értesítés üzenete
                .SetSmallIcon(Resource.Drawable.notifi_icon)  // Ellenőrizd, hogy létezik ez az ikon
                .SetAutoCancel(enableKill)       // Értesítés automatikusan eltűnik kattintás után
                .SetContentIntent(pendingIntent) // Kattintásra elindítja az aktivitást
                .Build();


            // NotificationChannel létrehozása Android O (API 26) vagy újabb verzión
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default);
                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

                // Notification megjelenítése
                 notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManager.Notify(notificationId, notification); // Megjelenítés Android O és újabb verziókon
            }
            else
            {
                // Notification megjelenítése
                var notificationManagerCompat = NotificationManagerCompat.From(context);
                 notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManagerCompat.Notify(notificationId, notification);

            }

            return notificationId;

        }

        /// <summary>
        /// Létrehoz és megjelenít egy modal értesítést a megadott csatornán.
        /// Az értesítés állandó!
        /// </summary>
        /// <param name="context">A Context példány, amely az értesítési szolgáltatások eléréséhez szükséges.</param>
        /// <param name="title">Az értesítés címe.</param>
        /// <param name="message">Az értesítés tartalma.</param>
        /// <param name="channelName">Az értesítési csatorna neve, amely az értesítések kategorizálására szolgál.</param>
        /// <param name="overwrite">
        /// Ha true, akkor az értesítés felülíródik egy előre megadott ID-val (987), 
        /// máskülönben egyedi, véletlenszerű ID-val jön létre, amely új értesítésként jelenik meg.
        /// </param>
        public static void NotificationModal(Context context, string title, string message, string channelName, bool overwrite, bool autoClose = false, int milliSec = 5000)
        {
            //Ha az alkalmazás előtérben van akkor kilép
            if (IsAppInForeground(context))
            {
                return;
            }

            //Activálás beállítása
            var intent = new Intent(context, typeof(MainActivity)); // A fő aktivitásod
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop); // A már meglévő aktivitás újrahasználása
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);

            var channelId = CreateChannelID(channelName);

            // Notification létrehozása
            var notification = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)    // Az értesítés címe
                .SetContentText(message)   // Az értesítés üzenete
                .SetSmallIcon(Resource.Drawable.notifi_icon)  // Ellenőrizd, hogy létezik ez az ikon
                .SetOngoing(true)           //Az értesítés állandó, nem lehet eltüntetni.
                 .SetContentIntent(pendingIntent) // Kattintásra elindítja az aktivitást
                .Build();

            // NotificationChannel létrehozása Android O (API 26) vagy újabb verzión
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default);
                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

                // Notification megjelenítése
                int notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManager.Notify(notificationId, notification); // Megjelenítés Android O és újabb verziókon
                 // Eltünteti az értesítést 
                if (autoClose)
                {
                    var handler = new Handler();
                    handler.PostDelayed(() =>
                    {
                        notificationManager.Cancel(notificationId); // Eltünteti az értesítést
                    }, milliSec); 
                }
            }
            else
            {
                // Notification megjelenítése
                var notificationManagerCompat = NotificationManagerCompat.From(context);
                int notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManagerCompat.Notify(notificationId, notification);
                // Eltünteti az értesítést 
                if (autoClose)
                {
                    var handler = new Handler();
                    handler.PostDelayed(() =>
                    {
                        notificationManagerCompat.Cancel(notificationId); // Eltünteti az értesítést
                    }, milliSec); 
                }

            }

        }

        public static void AlarmNotification(Context context, string title, string message, string channelName, bool enableKill, bool overwrite)
        {

			//Activálás beállítása
			var fullScreenIntent = new Intent(context, typeof(MainActivity));
			fullScreenIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop | ActivityFlags.NewTask);
			var pendingIntent = PendingIntent.GetActivity(context, 0, fullScreenIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);



			var channelId = "alarm_channel";

            var soundUri = Android.Net.Uri.Parse($"android.resource://{Android.App.Application.Context.PackageName}/Raw/police_siren.mp3");
            
            // Notification létrehozása
            var notification = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)    // Az értesítés címe
                .SetContentText(message)   // Az értesítés üzenete
                .SetSmallIcon(Resource.Drawable.notifi_icon)  // Ellenőrizd, hogy létezik ez az ikon
                .SetVibrate(new long[] { 0, 500, 1000, 500 })                     // Rezgés minta
                .SetAutoCancel(enableKill)       // Értesítés automatikusan eltűnik kattintás után
                .SetPriority(NotificationCompat.PriorityMax) // Magas prioritású értesítések(azonosítja, hogy megjelenik-e az értesítési sávban, ha nem a foregroundban vagy).
				.SetCategory(NotificationCompat.CategoryAlarm) // Fontos, hogy az Android prioritásosan kezelje!
                .SetFullScreenIntent(pendingIntent,true)
				//.SetContentIntent(pendingIntent) // Kattintásra elindítja az aktivitást
                .Build();


            // NotificationChannel létrehozása Android O (API 26) vagy újabb verzión
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High);
				channel.EnableLights(true);
				channel.EnableVibration(true);
				channel.SetVibrationPattern(new long[] { 0, 500, 1000, 500 });
				channel.SetSound(soundUri, new AudioAttributes.Builder()
					.SetUsage(AudioUsageKind.Alarm)
					.SetContentType(AudioContentType.Sonification)
					.Build());


				var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

                // Notification megjelenítése
                int notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManager.Notify(notificationId, notification); // Megjelenítés Android O és újabb verziókon
            }
            else
            {
                // Notification megjelenítése
                var notificationManagerCompat = NotificationManagerCompat.From(context);
                int notificationId = overwrite ? 987 : new Random().Next(1000, 9999); // Ha overwrite true, akkor ugyanaz az ID (987), különben véletlenszerű ID
                notificationManagerCompat.Notify(notificationId, notification);

            }
        }

        public static void CloseNotification(Context context, int notificationId)
        {
            var notificationManagerCompat = NotificationManagerCompat.From(context);
            notificationManagerCompat.Cancel(notificationId);
        }

        public static bool IsAppInForeground(Context context)
        {
            var activityManager = (ActivityManager)context.GetSystemService(Context.ActivityService);
    
            if (activityManager == null)
            {
                // Ha activityManager null, az alkalmazás állapotát nem tudjuk ellenőrizni
                return false;
            }

            var runningAppProcesses = activityManager.RunningAppProcesses;
    
            if (runningAppProcesses == null)
            {
                // Ha a runningAppProcesses lista null, nincs elérhető adat a folyamatokról
                return false;
            }

            foreach (var process in runningAppProcesses)
            {
                if (process.Importance == Importance.Foreground && process.ProcessName == context.PackageName)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
#endif

