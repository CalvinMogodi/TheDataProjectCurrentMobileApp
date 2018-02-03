using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace TheDataProject.Droid.Helpers
{
    public class GPSTracker 
    {
        private Context mContext;

        // flag for GPS status
        bool isGPSEnabled = false;

        // flag for network status
        bool isNetworkEnabled = false;

        // flag for GPS status
        bool canGetLocation = false;

        Location location = null; // location
        double latitude; // latitude
        double longitude; // longitude

        // The minimum distance to change Updates in meters
        private static long MIN_DISTANCE_CHANGE_FOR_UPDATES = 10; // 10 meters

        // The minimum time between updates in milliseconds
        private static long MIN_TIME_BW_UPDATES = 1000 * 60 * 1; // 1 minute

        // Declaring a Location Manager
        protected LocationManager locationManager;

        public Location GetLocation(Context context)
        {
            try
            {
                this.mContext = context;
                locationManager = (LocationManager)mContext.GetSystemService("location");
                // getting GPS status

                bool isGPSEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
                bool isNetworkEnabled = locationManager.IsProviderEnabled(LocationManager.NetworkProvider);
                bool isPassiveProviderEnabled = locationManager.IsProviderEnabled(LocationManager.PassiveProvider);

                if (!isGPSEnabled && !isNetworkEnabled && !isPassiveProviderEnabled)
                {
                    MessageDialog messageDialog = new MessageDialog();
                    messageDialog.SendToast("GPS is not enabled");
                    return null;
                }
                else
                {
                    canGetLocation = true;
                    if (isGPSEnabled)
                    {
                        location = locationManager.GetLastKnownLocation(LocationManager.GpsProvider);
                    }
                    if (isNetworkEnabled)
                    {
                        if (location == null)
                            location = locationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
                    }
                    if (isPassiveProviderEnabled)
                    {
                        if (location == null)
                            location = locationManager.GetLastKnownLocation(LocationManager.PassiveProvider);
                    }
                }

            }
            catch (Exception e)
            {
                //e.printStackTrace();
            }

            return location;
        }
        
    }
}