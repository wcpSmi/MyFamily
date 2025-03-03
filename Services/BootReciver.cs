using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFamily
{
#if ANDROID

    using Android.OS;
    using Android.App;
    using Android.Content;

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]

    //Rendszerindítás figyelése
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

            if (intent.Action == Intent.ActionBootCompleted)
            {
                try
                {
                    // Indítsd el a BackgroundService-t a rendszerindítás után
                    var serviceIntent = new Intent(context, typeof(BackgroundService));
                    var serviceLocation = new Intent(context, typeof(LocationService));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.P) // Android 9.0 vagy újabb esetén
                    {
                        context.StartForegroundService(serviceIntent);
                        context.StartForegroundService(serviceLocation);
                        //context.StartService(serviceIntent);  
                        //context.StartService(serviceLocation);

                    }
                    else // Android 7.1 vagy régebbi verziók esetén
                    {
                        context.StartService(serviceIntent);
                        context.StartService(serviceLocation);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

        }

    }

#endif
}
