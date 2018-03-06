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
            string locationProvider = "gps";
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
            }
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
                if (isPassiveProviderEnabled)
                {
                    if (location == null)
                        location = locationManager.GetLastKnownLocation(LocationManager.PassiveProvider);
                }
                if (isNetworkEnabled)
                {
                    if (location == null)
                        location = locationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
                }
                         
            }
            return location;
        }

        public string InitializeLocationManager()
        {
            LocationManager _locationManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
               return  acceptableLocationProviders.First();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}