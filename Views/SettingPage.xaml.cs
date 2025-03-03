#if ANDROID
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android;
using Android.App;
using Android.Content.PM;
#endif
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;



namespace MyFamily;

public partial class SettingPage : ContentPage
{
	private readonly Action<bool> onSettingsClosed;

	public SettingPage(Action<bool> settingsClosed)
	{
		InitializeComponent();
		onSettingsClosed = settingsClosed;

		UserNameEntry.Text = AppSetting.UserName;
		ConnectionStringEntry.Text = AppSetting.ConnectionString;
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{

		if (CheckAndSaveUserName(UserNameEntry.Text) && await CheckAndSaveConnectionString(ConnectionStringEntry.Text))
		{

			// Szolgáltatás újraindítása, ha érvényes Connection String-et kaptunk
			RestartBackgroundService();

			await DisplayAlert("Saved", "New settings has been saved.", "OK");
			// Bezárjuk a SettingPage-t, és értesítjük a MainPage-et, hogy sikeresen mentettük
			onSettingsClosed?.Invoke(true);
			await Navigation.PopModalAsync();

		}
		else
		{
			await DisplayAlert("Error", "An error while saving settings. \n(Username and cannot be empty, connection string must be valid) ", "OK");
			onSettingsClosed?.Invoke(false);
		}
	}

	private async Task<bool> CheckAndSaveConnectionString(string conStr)
	{
		if (!string.IsNullOrEmpty(conStr))
		{

			RedisService.TryConnect(conStr);
			if (!RedisService.IsConnectSuccess)
			{
				await DisplayAlert("Hiba", "Nem lehet csatlakozni a Redis szerverhez!", "OK");
				return false;
			}

			AppSetting.ConnectionString = conStr;
			Preferences.Set("ConnectionString", conStr);
			return true;
		}
		return false;
	}



	private void RestartBackgroundService()
	{
#if ANDROID
        var context = Android.App.Application.Context;
        var intent = new Intent(context, typeof(BackgroundService));
        context.StopService(intent);
        context.StartForegroundService(intent);
#endif
	}

	private bool CheckAndSaveUserName(string userName)
	{
		if (!string.IsNullOrEmpty(userName))
		{
			AppSetting.UserName = userName;
			Preferences.Set("UserName", userName);
			return true;
		}
		return false;
	}
	private async Task RequestPostNotificationsPermission()
	{
		// Ellenõrizzük, hogy a jogosultság engedélyezve van-e
		var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

		if (status == PermissionStatus.Granted)
		{
			// A jogosultság már meg van adva
			return;
		}

		if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
		{
			// Kérjük a jogosultságot
			status = await Permissions.RequestAsync<Permissions.PostNotifications>();
		}

		if (status != PermissionStatus.Granted)
		{
			// Ha nem kaptuk meg a jogosultságot, informáljuk a felhasználót
			await DisplayAlert("Permission Denied", "Please enable notifications in settings.", "OK");
		}
	}


	private void OpenNotificationPolicyAccessSettings(object sender, EventArgs e)
	{
#if ANDROID
		var intent = new Intent(Android.Provider.Settings.ActionNotificationPolicyAccessSettings);
		Platform.CurrentActivity.StartActivity(intent);
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}

	private void OpenIgnoreBatteryOptimizationSettings(object sender, EventArgs e)
	{
#if ANDROID
		var intent = new Intent(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
		Platform.CurrentActivity.StartActivity(intent);
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}

	private void OpenManageAllFilesAccessPermission(object sender, EventArgs e)
	{
#if ANDROID
		var intent = new Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
		intent.SetData(Android.Net.Uri.Parse($"package:{Android.App.Application.Context.PackageName}"));
		Platform.CurrentActivity.StartActivity(intent);
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}


	private void OpenApplicationDetailsSettings(object sender, EventArgs e)
	{
#if ANDROID
		var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
		intent.SetData(Android.Net.Uri.Parse($"package:{Android.App.Application.Context.PackageName}"));
		Platform.CurrentActivity.StartActivity(intent);
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}

	private void OpenAppNotificationSettings(object sender, EventArgs e)
	{
#if ANDROID
		var intent = new Intent(Android.Provider.Settings.ActionAppNotificationSettings);

		if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
		{
			intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, Android.App.Application.Context.PackageName);
		}
		else
		{
			intent.PutExtra("app_package", Android.App.Application.Context.PackageName);
			intent.PutExtra("app_uid", Android.App.Application.Context.ApplicationInfo.Uid);
		}

		Platform.CurrentActivity.StartActivity(intent);
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}


	private void OpenDeviceSpecificSettings(object sender, EventArgs e)
	{
#if ANDROID
		try
		{
			var manufacturer = Build.Manufacturer.ToLower();
			var intents = new List<Android.Content.Intent>();

			switch (manufacturer)
			{
				case "xiaomi":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity")));
					break;

				case "huawei":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity")));
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.startupmgr.ui.StartupNormalAppListActivity")));
					break;

				case "oppo":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity")));
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.startupapp.StartupAppListActivity")));
					break;

				case "vivo":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.AddWhiteListActivity")));
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.BgStartUpManager")));
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity")));
					break;

				case "samsung":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.samsung.android.lool", "com.samsung.android.sm.ui.battery.BatteryActivity")));
					break;

				case "asus":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.asus.mobilemanager", "com.asus.mobilemanager.MainActivity")));
					break;

				case "letv":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity")));
					break;

				case "htc":
					intents.Add(new Android.Content.Intent().SetComponent(new ComponentName("com.htc.pitroad", "com.htc.pitroad.landingpage.activity.LandingPageActivity")));
					break;

				default:
					// Alapértelmezett: Nyisd meg az általános beállításokat
					intents.Add(new Android.Content.Intent(Android.Provider.Settings.ActionSettings));
					break;
			}

			foreach (var intent in intents)
			{
				var resolveInfoList = Platform.CurrentActivity.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
				if (resolveInfoList.Count > 0)
				{
					Platform.CurrentActivity.StartActivity(intent);
					//return; // Csak az elsõ sikeres ablakot nyitjuk meg
				}
			}

			// Ha egyetlen intent sem sikerült, próbálj meg egy alapértelmezett beállítási oldalt
			Platform.CurrentActivity.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionSettings));
		}
		catch (Exception ex)
		{
			Android.Util.Log.Error("DeviceSettings", $"Hiba történt a beállítások megnyitásakor: {ex.Message}");
		}
#else
		DisplayAlert("Figyelem !", "Ez a funkció csak Androidon mûködik", "ok");
#endif
	}

}


