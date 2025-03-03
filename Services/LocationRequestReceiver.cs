using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFamily
{
#if ANDROID
    using Android.App;
    using Android.Content;

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "com.wcp.myfamily.LOCATION_REQUEST" })] // Testreszabható intent filter
    public class LocationRequestReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (!LocationServiceState.IsServiceRunning)
            {
                if (intent.Action == "com.wcp.myfamily.LOCATION_REQUEST")
                {
                    // Indítsd el a LocationService-t
                    LocationService.StartService(context);
                }
            }
        }
    }
#endif
}

