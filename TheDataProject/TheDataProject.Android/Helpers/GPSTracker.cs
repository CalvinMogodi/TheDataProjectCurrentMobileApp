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
        public bool isLocationGPSEnabled = true;
        public Location GPSCoordinate() {

            LocationManager locationManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            bool isGPSEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            bool isNetworkEnabled = locationManager.IsProviderEnabled(LocationManager.NetworkProvider);
            bool isPassiveProviderEnabled = locationManager.IsProviderEnabled(LocationManager.PassiveProvider);
            var locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Fine;
            locationCriteria.PowerRequirement = Power.Medium;

            string locationProvider = locationManager.GetBestProvider(locationCriteria, true);
            Location location = null;
            if (!isGPSEnabled && !isNetworkEnabled && !isPassiveProviderEnabled)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("GPS is not enabled");
                isLocationGPSEnabled = false;
            }
            else
            {
                if (!String.IsNullOrEmpty(locationProvider))
                {
                    location = locationManager.GetLastKnownLocation(locationProvider);
                }
                if (isGPSEnabled)
                {
                    if (location == null)
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
            return location;
        }
    }
}