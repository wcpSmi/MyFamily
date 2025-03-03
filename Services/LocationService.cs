#if ANDROID
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Android.App;
using Android.Content;
using MyFamily;
using CommunityToolkit.Mvvm.Messaging;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using AndroidX.ConstraintLayout.Motion.Utils;


[assembly: Dependency(typeof(LocationService))]  // DependencyService regisztráció

namespace MyFamily
{
    public class LocationServiceState
    {
        // Statikus flag, hogy a szolgáltatás fut-e
        public static bool IsServiceRunning = false;
    }
    
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {

            // Ha Android 8.0 vagy újabb verzió van, indítsuk el előtérszolgáltatásként
            //StartService(this);

            // Csak akkor indítjuk el a szolgáltatást, ha még nem fut
            if (!LocationServiceState.IsServiceRunning)
            {
                LocationServiceState.IsServiceRunning = true;
                StartForegroundService();
                Task.Delay(100);
            }
            
            LocationServiceState.IsServiceRunning = false; // A szolgáltatás leállítása után reseteljük a flag-et
            


            return StartCommandResult.NotSticky;  // A szolgáltatás nem indul újra automatikusan
												  //return StartCommandResult.Sticky;  // Megtartjuk a szolgáltatást a háttérben

		}

        public static void StopService(Context context)
        {
            var intent = new Intent(context, typeof(LocationService));
            context.StopService(intent); // Leállítja a szolgáltatást
            LocationServiceState.IsServiceRunning = false; // Flag visszaállítása

		}

        public static void StartService(Context context)
        {
            var intent = new Intent(context, typeof(LocationService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }

            
        }


        private void StartForegroundService()
        {
			var channelId = "location_service_channel";

			var notification = new NotificationCompat.Builder(this, channelId)
				.SetSmallIcon(Resource.Drawable.notifi_icon_bw)
				.SetPriority(NotificationCompat.PriorityMin) 
				.SetOngoing(true)
				.SetSilent(true) 
				.SetContentText("Tracking location in background")
				.Build();

			StartForeground(2, notification);


		}

        public override IBinder OnBind(Intent intent)
        {
            return null; // Mivel nem szeretnénk klienst kötni ehhez a szolgáltatáshoz, null-t adunk vissza.
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Itt állíthatod le a helyzetfigyelési folyamatokat vagy felszabadíthatod az erőforrásokat
            LocationServiceState.IsServiceRunning = false;
        }
    }
}
#endif

