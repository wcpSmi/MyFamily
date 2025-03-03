using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Maps;
using System.Net.NetworkInformation;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.PlatformConfiguration;




namespace MyFamily
{

    public partial class Map : ContentPage
    {
        private readonly Action<bool> onMapClosed;

        public Map(Action<bool> mapClosed, Message message, string user)
        {
            InitializeComponent();
            onMapClosed = mapClosed;


            JeloloElhelyezese(message);
        }

        private async void JeloloElhelyezese(Message message)
        {
            var address = message.Text;
            if (address != null)
            {
                label1.Text = address;
                var marker = new Pin
                {
                    Address = address,
                    //Icon = "marker",
                    Location = message.Location,// new Location(e.Location.Latitude, e.Location.Longitude),
                    Type = PinType.Place,                  
                    Label = message.SenderName
                    
                };
                
                myMap.Pins.Add(marker);


                // Térkép középpontjának és zoom-nak beállítása
                myMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(message.Location.Latitude, message.Location.Longitude), Distance.FromKilometers(1)));

            }
            else
            {
                label1.Text = "Cím nem található!";
            }
        }


        private async void OnExitClicked(object sender, EventArgs e)
        {
            // Bezárjuk a Map-ot, és értesítjük a MainPage-et, hogy sikeresen mentettük
            onMapClosed?.Invoke(true);
            await Navigation.PopModalAsync();
        }

        private void OnGoogleUtvonalClicked(object sender, EventArgs e)
        {
            GoogleMapInicializalas(); // Meghívjuk a metódust
        }

        private void GoogleMapInicializalas()
        {
            string address = label1.Text; // A cím a Label-ből
            string encodedAddress = Uri.EscapeDataString(address);
            var geoUri = $"geo:0,0?q={encodedAddress}";

            try
            {
                Launcher.OpenAsync(new Uri(geoUri)); // Google Maps indítása
            }
            catch (Exception ex)
            {
                DisplayAlert("Hiba", $"Nem sikerült megnyitni a Google Maps alkalmazást: {ex.Message}", "OK");
            }
        }
        public void MoveToPosition(double latitude, double longitude)
        {
            if (myMap != null)
            {
                // Áthelyezzük a térképet a megadott pozícióra és beállítjuk a zoom szintet
                myMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(latitude, longitude), Distance.FromKilometers(1))
                );
            }
        }

        public void StopMap()
        {
            OnDisappearing();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Tisztítsd meg a térképet
            myMap.Pins.Clear(); // Eltávolít minden jelölőt
            // Helyzetmeghatározás leállítása
            myMap.IsShowingUser = false;// Kikapcsolja az aktuális helyzet mutatását
        }
    }
}