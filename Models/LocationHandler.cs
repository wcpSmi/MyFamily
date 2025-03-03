#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
#endif
using Microsoft.Maui.Devices.Sensors;

using System.Threading.Tasks;


using Microsoft.Maui.Devices.Sensors;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;

namespace MyFamily
{
    public static class LocationHandler
    {

        public static Location? Location { get; set; }
        public static string? Address { get; set; }


        public static async Task<(string, Location)> LocationData()
        {
            try
            {
                // Ellenőrizd, hogy van-e helymeghatározási engedély
                var permission = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (permission != PermissionStatus.Granted)
                {
                    return ("Nincs engedély a helymeghatározáshoz.", new Location(0, 0));
                }


                // Új hely kérése, ha nincs ismert hely
                Location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(1)
                });

                if (Location == null)
                {
                    return ("Hely nem elérhető", new Location(0, 0));
                }


                // Fordított geokódolás a cím meghatározásához
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(Location);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    Address = $"{placemark.Locality} {placemark.PostalCode}, {placemark.Thoroughfare} {placemark.SubThoroughfare}, ({placemark.CountryName})";

                    return (Address, Location);
                }

                // Ha nincs cím, térítsük vissza csak a koordinátákat
                return ("Cím nem elérhető", Location);
            }
            catch (FeatureNotSupportedException)
            {
                return ("Helymeghatározás nem támogatott ezen az eszközön.", new Location(0, 0));
            }
            catch (PermissionException)
            {
                return ("Nincs engedély a helymeghatározáshoz.", new Location(0, 0));
            }
            catch (Exception ex)
            {
                return ($"Hiba történt: {ex.Message}", new Location(0, 0));
            }
        }
    }


}











