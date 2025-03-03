using CommunityToolkit.Mvvm;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;



#if IOS
using UserNotifications;
using static System.Net.Mime.MediaTypeNames;
#endif
#if ANDROID
using Android.Content;
using System.Runtime.Versioning;
using Android.OS;
using Android.Provider;
using Android.OS;
using Java.Util;




#endif

namespace MyFamily
{

	public partial class MainPage : ContentPage
	{
		public static MainPage Instance { get; private set; }  // Statikus példány
		public static Map CurrentMap { get; set; }

		private TaskCompletionSource<bool> settingsTaskCompletionSource;

		public ObservableCollection<Message> CollectionItems { get; set; } = new ObservableCollection<Message>();
		private bool isStart = true;

		public MainPage()
		{

			InitializeComponent();

			Instance = this;

			//Android engedély kérések
			AndroidBackgroundPermission();

			BindingContext = this;

			AppSetting.CheckAppSetting(Instance);

		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			if (isStart)
			{
				await Task.Delay(500);
				OnWhereAreYouClicked(null, null);
			}

			isStart = false;
		}

		private void AndroidBackgroundPermission()
		{
#if ANDROID
			// Csak Androidon érhető el a BackgroundService indítása
			var intent = new Intent(Android.App.Application.Context, typeof(BackgroundService));
			Android.App.Application.Context.StartService(intent);
#endif
		}

		private async void ScrollToLastMessage()
		{
			// Ellenőrizd, hogy van-e üzenet
			if (CollectionItems.Count > 0)
			{
				await Task.Delay(100); // Késleltetés a gördüléshez

				var lastMessage = CollectionItems.Last();
				// Görgess a CollectionItems legalsó eleméhez
				MessagesCollectionView.ScrollTo(lastMessage, position: ScrollToPosition.End);

			}
		}
		private void OnScrollToLastMessage(object sender, EventArgs e)
		{
			ScrollToLastMessage();
		}

		// Eseménykezelő a "Menu" gombhoz
		public async void OnMenuClicked(object sender, EventArgs e)
		{

			// Inicializáljuk a TaskCompletionSource-t
			if (settingsTaskCompletionSource == null)
			{
				settingsTaskCompletionSource = new TaskCompletionSource<bool>();
			}

			settingsTaskCompletionSource = new TaskCompletionSource<bool>();

			// Megnyitjuk a SettingPage oldalt modálisan
			await Navigation.PushModalAsync(new SettingPage(OnSettingsClosed));

			// Várakozás, amíg a SettingPage be nem zárul
			bool success = await settingsTaskCompletionSource.Task;

			// Itt folytathatjuk a további műveleteket, miután visszatért a beállítások oldalról
			if (success)
			{
				// A felhasználónév módosult, frissítjük a címet
				this.Title = "Family - " + AppSetting.UserName;
			}
		}

		private void OnSettingsClosed(bool success)
		{
			if (settingsTaskCompletionSource != null && !settingsTaskCompletionSource.Task.IsCompleted)
			{
				// A TaskCompletionSource jelez, hogy a visszatérés megtörtént
				settingsTaskCompletionSource.SetResult(success);
			}
			OnWhereAreYouClicked(null, null);
		}

		private void OnMapClosed(bool success)
		{
			if (settingsTaskCompletionSource != null && !settingsTaskCompletionSource.Task.IsCompleted)
			{
				// A TaskCompletionSource jelez, hogy a visszatérés megtörtént
				settingsTaskCompletionSource.SetResult(success);

#if ANDROID

				if (LocationServiceState.IsServiceRunning)
				{
					LocationService.StopService(Android.App.Application.Context);
				}
#endif
			}

		}



		private async void ShowMap(Message message)
		{
			// Inicializáljuk a TaskCompletionSource-t
			settingsTaskCompletionSource = new TaskCompletionSource<bool>();

			// Megnyitjuk a SettingPage oldalt modálisan
			CurrentMap = new Map(OnMapClosed, message, message.SenderName);
			await Navigation.PushModalAsync(CurrentMap);

			// Várakozás, amíg a SettingPage be nem zárul
			bool success = await settingsTaskCompletionSource.Task;

			// Itt folytathatjuk a további műveleteket, miután visszatért a beállítások oldalról
			if (success)
			{
				// A felhasználónév módosult, frissítjük a címet
				this.Title = "HOME - " + AppSetting.UserName;
			}
			// Kikapcsoljuk az aktuális helyzet mutatását
			// Ha a CurrentMap nem null, akkor leállítjuk a helyzetmeghatározást
			CurrentMap?.StopMap();
		}

		private async void OnLabelTapped(object sender, EventArgs e)
		{
			if (sender is Label label)
			{
				var text = label.Text;
				// Szülő keresése
				var parent = label.Parent;
				while (parent != null)
				{
					// Típusellenőrzés
					if (parent is Frame frame)
					{

						if (frame.BindingContext is Message message)
						{
							if (message.Text == null || message.SenderName == "Keresés...")
							{
								return;
							}

							// Tetszőleges művelet, például a szöveg másolása a vágólapra
							await DisplayAlert("Address copied to clipboard!", message.Text, "ok");
							await Clipboard.SetTextAsync(message.Text);

							ShowMap(message);
						}
					}
					parent = parent.Parent;
				}
			}
		}

		private async void OnMessageTapped(object sender, EventArgs e)
		{
			if (sender is Frame frame)
			{

				var message = frame.BindingContext as Message;
				if (message.Host == string.Empty || message.SenderName == "Keresés...")
				{
					return;
				}

				var text = message.Text;
				await DisplayAlert("Address copied to clipboard!", message.Text, "ok");
				await Clipboard.SetTextAsync(message.Text);

				ShowMap(message);
			}

		}


		private async void OnWhereAreYouClicked(object sender, EventArgs e)
		{
			CollectionItems.Clear();

			//Helyadat lekérése
			await RedisService.SendMessageAsync(AppSetting.RedisChannel, MessageHandler.GetLocationQueryMessage(), false); 

			//ha nincs eredmény
			if (CollectionItems.Count == 0)
			{
				MessageShow(new Message
				{
					SenderName = "Keresés...",
					Text = "Nincs csatlakozott felhasználó !",
					BackColor = Colors.LightYellow,
					Date = DateTime.Now.ToString(),
					CanSwipe = false
				});
			}
		}


		//TODO Fejlesztés:  a korábbi helyadatok lekérdezése és kiirása a listára


		public void HandleReceivedMessage(Message message)
		{
			//háttérszin visszaalakítása
			message.BackColor = Color.FromArgb(message.BackColor.ToHex());

			if (message.Host != message.SenderUUID && message.Host == AppSetting.UUID)
			{
				var itemRemove = CollectionItems.FirstOrDefault(m => m.SenderName == "Keresés...");
				if (itemRemove != null)
					CollectionItems.Remove(itemRemove);

				MessageShow(message);
			}

#if ANDROID
			NotificationHelper.CloseNotification(Android.App.Application.Context, 987);
#endif
			ScrollToLastMessage(); // Görgetés az utolsó üzenethez

		}


		public void MessageShow(Message message)
		{
			CollectionItems.Add(message);

		}

		private void OnSwipeStarted(object sender, SwipeStartedEventArgs e)
		{
			if (sender is SwipeView swipeView)
			{
				var message = swipeView.BindingContext as Message;

				// Ha a CanSwipe false, akkor visszazárjuk az elemet
				if (message != null && !message.CanSwipe)
				{
					swipeView.Close();  // Swipe visszazárása
				}
			}
		}

		private Message GetSwipeAsUser(object sender)
		{
			var item = sender as SwipeItem;
			if (item is null)
				return null;


			var user = item.BindingContext as Message;
			if (user is null)
				return null;

			return user;
		}
		private async void SwipeItem_Alarm(object sender, EventArgs e)
		{
			var user = GetSwipeAsUser(sender);
			if (user.SenderName != "Keresés...")
			{
				var message = await MessageHandler.GetAlarmMessage(user);
				await RedisService.SendMessageAsync(AppSetting.RedisChannel, message, false);
			}
		}


		private async void ShowCustomAlert(string title, string text)
		{
			await Navigation.PushModalAsync(new CustomAlertPage(title, text));
		}

	}
}

