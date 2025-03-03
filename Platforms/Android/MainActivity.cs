#if ANDROID
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using static Microsoft.Maui.ApplicationModel.Platform;
#endif

namespace MyFamily
{

    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CreateNotificationChannels();
            SettupWindow();
            RequestNecessaryPermissions();
        }

        private void CreateNotificationChannels()
        {
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var manager = (NotificationManager)GetSystemService(NotificationService);

				// Location Service Channel
				var locationChannel = new NotificationChannel(
					"location_service_channel", "Location Service", NotificationImportance.Min)
				{
					Description = "Used for location tracking service"
				};
				locationChannel.EnableLights(false);
				locationChannel.EnableVibration(false);
				manager.CreateNotificationChannel(locationChannel);

				// Background Service Channel
				var backgroundChannel = new NotificationChannel(
					"background_service_channel", "Background Service", NotificationImportance.Low)
				{
					Description = "Used for background processing"
				};
				backgroundChannel.EnableLights(false);
				backgroundChannel.EnableVibration(false);
				manager.CreateNotificationChannel(backgroundChannel);

				// Alarm Notification Channel
				var alarmSoundUri = Android.Net.Uri.Parse($"android.resource://{PackageName}/raw/police_siren");

				var alarmChannel = new NotificationChannel(
					"alarm_channel", "Alarm Channel", NotificationImportance.High)
				{
					Description = "Used for urgent SOS messages"
				};
				alarmChannel.EnableLights(true);
				alarmChannel.EnableVibration(true);
				alarmChannel.SetVibrationPattern(new long[] { 0, 500, 1000, 500 });
				alarmChannel.SetSound(alarmSoundUri, new AudioAttributes.Builder()
					.SetUsage(AudioUsageKind.Alarm)
					.SetContentType(AudioContentType.Sonification)
					.Build());

				manager.CreateNotificationChannel(alarmChannel);
			}
		}
        

        private void SettupWindow()
        {
            //Helyszolgáltatások beállítása ablak---------------------------------------------
            var locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.LocationService);

            // Ellenőrizzük, hogy a GPS és/vagy a hálózat-alapú helyszolgáltatások engedélyezettek-e
            bool isGpsEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            bool isNetworkEnabled = locationManager.IsProviderEnabled(LocationManager.NetworkProvider);

            if ((!isGpsEnabled || !isNetworkEnabled))
            {
                // Helyszolgáltatások nincsenek bekapcsolva, irányítsuk át a beállítások képernyőre
                var builder = new AlertDialog.Builder(this);
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);

                    builder.SetTitle("Helymeghatározás szükséges")
                           .SetMessage("Kapcsold be a helymeghatározási szolgáltatásokat (GPS vagy hálózat)!")
                           .SetPositiveButton("Beállítások", (sender, args) =>
                           {
                              
                               StartActivity(intent);
                           })
                           .SetNegativeButton("Kilépés", (sender, args) =>
                           {
                               // Alkalmazás bezárása
                               FinishAffinity(); // Bezárja az összes Activity-t
                               Java.Lang.JavaSystem.Exit(0); // Kilép az alkalmazásból
                           })
                           .SetCancelable(false) // Ne engedje a párbeszédablakot háttérbe tenni
                           .Show();
                
            }
            else
            {
                // Helymeghatározás elérhető
                Toast.MakeText(Android.App.Application.Context, "A helymeghatározás be van kapcsolva.", ToastLength.Long).Show();
            }

        }

        private bool IsAutoStartEnabled()
        {
            // Ellenőrizd, hogy az alkalmazás rendelkezik-e autostart engedéllyel
            // Ez készüléktől függően eltérhet, így esetleg a Huawei és Xiaomi modellek különböző API-it kell használni
            var pm = PackageManager;
            var actionIntent = new Android.Content.Intent(Android.Provider.Settings.ActionManageOverlayPermission);
            var activityList = pm.QueryIntentActivities(actionIntent, PackageInfoFlags.MatchDefaultOnly);

            return activityList.Count > 0;
        }


        //Tudás tár !!! Ne töröld !!!
        //new Intent().setComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity")), 
        //new Intent().setComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity")), 
        //new Intent().setComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.startupmgr.ui.StartupNormalAppListActivity")), 
        //new Intent().setComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity")), 
        //new Intent().setComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.appcontrol.activity.StartupAppControlActivity")), 
        //new Intent().setComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity")), 
        //new Intent().setComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.startupapp.StartupAppListActivity")), 
        //new Intent().setComponent(new ComponentName("com.oppo.safe", "com.oppo.safe.permission.startup.StartupAppListActivity")), 
        //new Intent().setComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.AddWhiteListActivity")), 
        //new Intent().setComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.BgStartUpManager")), 
        //new Intent().setComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity")), 
        //new Intent().setComponent(new ComponentName("com.samsung.android.lool", "com.samsung.android.sm.ui.battery.BatteryActivity")), 
        //new Intent().setComponent(new ComponentName("com.htc.pitroad", "com.htc.pitroad.landingpage.activity.LandingPageActivity")), 
        //new Intent().setComponent(new ComponentName("com.asus.mobilemanager", "com.asus.mobilemanager.MainActivity"))))


        private void RequestNecessaryPermissions()
        {


             if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 1);
            }

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted ||
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[]
                {
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.AccessCoarseLocation
                }, 2);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessBackgroundLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessBackgroundLocation }, 3);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.WriteExternalStorage }, 0);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            string result = "";
            for (int i = 0; i < permissions.Length; i++)
            {
                //Console.WriteLine($"{permissions[i]}: {grantResults[i]}");
                result += $"{permissions[i]}: {grantResults[i]}";
            }

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

			if (MainPage.CurrentMap != null)
			{
				MainPage.CurrentMap.StopMap();
			}
			// Szolgáltatás leállítása
			var stopServiceIntent = new Android.Content.Intent(this, typeof(LocationService));
            StopService(stopServiceIntent);  // A LocationService leállítása

        }

    }

}

