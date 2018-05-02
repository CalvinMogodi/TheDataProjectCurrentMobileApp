using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using TheDataProject.Droid.Helpers;
using TheDataProject.Models;
using TheDataProject.ViewModels;
using static Android.Widget.AdapterView;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Location", ScreenOrientation = ScreenOrientation.Portrait)]
    public class LocationActivity : BaseActivity
    {
        protected override int LayoutResource => Resource.Layout.activity_location;
        #region Properties 

        ImageView gpsLocationButton, bpLocationButton, refashAccuracy;
        EditText streetAddress, suburb, region;
        Spinner province, localmunicipality;
        TextView tvfLatLong, boundaryPolygonsText, accuracyMessage;
        ListView bpListView;
        List<string> itemList;
        public GPSCoordinate _GPSCoordinates;
        public List<BoundryPolygon> _BoundryPolygons;
        ArrayAdapter<string> arrayAdapter;
        int oldPosition = 999999;
        Location Location;
        Facility Facility;
        private AppPreferences appPreferences;
        private UIHelpers helpers;
        public LocationViewModel ViewModel { get; set; }

        #endregion #endregion 

        #region Methods 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            streetAddress = FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = FindViewById<EditText>(Resource.Id.etf_suburb);
            region = FindViewById<EditText>(Resource.Id.etf_region);
            province = FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            gpsLocationButton = FindViewById<ImageView>(Resource.Id.gpscaddlocation_button);
            bpLocationButton = FindViewById<ImageView>(Resource.Id.bpaddlocation_button);
            refashAccuracy = FindViewById<ImageView>(Resource.Id.refreshaccuracy_button);
            tvfLatLong = FindViewById<TextView>(Resource.Id.tvf_latLang);
            boundaryPolygonsText = FindViewById<TextView>(Resource.Id.boundaryPolygonsText);
            accuracyMessage = FindViewById<TextView>(Resource.Id.accuracy_message);
            bpListView = FindViewById<ListView>(Resource.Id.bplistView1);
            itemList = new List<string>();
            refashAccuracy.Click += RefashAccuracy_Click;
            _BoundryPolygons = new List<BoundryPolygon>();
            ViewModel = new LocationViewModel();
            appPreferences = new AppPreferences(Application.Context);
            helpers = new UIHelpers();
            Location = new Models.Location();
            var data = Intent.GetStringExtra("data");
            if (data != null)
            {
                Facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                Location = Facility.Location;
                if (Location != null)
                {
                    streetAddress.Text = Location.StreetAddress;
                    suburb.Text = Location.Suburb;
                    region.Text = Location.Region;
                    province.SetSelection(helpers.GetSpinnerIndex(province, Location.Province));
                    localmunicipality.SetSelection(helpers.GetSpinnerIndex(localmunicipality, Location.LocalMunicipality));
                    if (Location.GPSCoordinates != null)
                    {
                        tvfLatLong.Text = "Lat: " + Location.GPSCoordinates.Latitude + " Long: " + Location.GPSCoordinates.Longitude;
                    }
                    if (Location.BoundryPolygon != null)
                    {
                        foreach (var BoundaryPolygon in Location.BoundryPolygon)
                        {
                            _BoundryPolygons.Add(BoundaryPolygon);
                            itemList.Add("Lat: " + BoundaryPolygon.Latitude.ToString() + " Long: " + BoundaryPolygon.Longitude.ToString());
                        }                        

                        arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, itemList);
                        bpListView.Adapter = arrayAdapter;
                        bpListView.ItemLongClick += Adapter_ItemSwipe;
                        ViewGroup.LayoutParams param = bpListView.LayoutParameters;
                        param.Height = 80 * _BoundryPolygons.Count();
                        bpListView.LayoutParameters = param;
                    }
                    boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                }
            }
            bpLocationButton.Click += BPLocationButton_Click;
            gpsLocationButton.Click += GPSLocationButton_Click;
            Toolbar.MenuItemClick += (sender, e) =>
            {
                var itemTitle = e.Item.TitleFormatted;
                switch (itemTitle.ToString())
                {
                    case "Log Out":
                        var intent = new Intent(this, typeof(LoginActivity));
                        appPreferences.SaveUserId("0");
                        StartActivity(intent);
                        break;
                    case "Save":
                        SaveLocation();
                        break;
                }
            };

            GPSTracker GPSTracker = new GPSTracker();
            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (location != null)
            {
                accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
            }
        }
               
        private void BPLocationButton_Click(object sender, EventArgs eventArgs)
        {
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
            else
            {
                BoundryPolygon BoundryPolygon = new BoundryPolygon()
                {
                    Latitude = location.Latitude.ToString(),
                    Longitude = location.Longitude.ToString()
                };
                _BoundryPolygons.Add(BoundryPolygon);
                itemList.Add("Lat: " + location.Latitude.ToString() + " Long: " + location.Longitude.ToString());

                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, itemList);
                bpListView.Adapter = arrayAdapter;
                bpListView.ItemLongClick += Adapter_ItemSwipe;
                ViewGroup.LayoutParams param = bpListView.LayoutParameters;
                param.Height = 80 * _BoundryPolygons.Count();
                bpListView.LayoutParameters = param;
            }

        }

        void Adapter_ItemSwipe(object sender, ItemLongClickEventArgs e)
        {
            if (_BoundryPolygons.Count() > 0)
            {
                if (oldPosition != e.Position)
                {
                    var item = arrayAdapter.GetItem(e.Position);
                    arrayAdapter.Remove(item);
                    itemList.Remove(item);
                    _BoundryPolygons.RemoveAt(e.Position);
                    boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                    oldPosition = e.Position;
                    bpListView.Adapter = arrayAdapter;
                    bpListView.ItemLongClick += Adapter_ItemSwipe;
                }                
            }
        }

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
        {
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }

            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
            else {
                tvfLatLong.Text = "Lat: " + location.Latitude.ToString() + " Long: " + location.Longitude.ToString();
                accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
                Location.GPSCoordinates = new GPSCoordinate() {
                    Longitude = location.Longitude.ToString(),
                    Latitude = location.Latitude.ToString()
                };
            }
        }

        private async void SaveLocation()
        {
            if (appPreferences.IsOnline(Application.Context))
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                if (!ValidateLocation())
                {
                    messageDialog.HideLoading();
                    return;
                }

                Location.LocalMunicipality = localmunicipality.SelectedItem.ToString();
                Location.Province = province.SelectedItem.ToString();
                Location.StreetAddress = streetAddress.Text;
                Location.Suburb = suburb.Text;
                Location.Region = region.Text;
                Location.BoundryPolygon = _BoundryPolygons;
                Location.FacilityId = Facility.Id;
                bool isSuccess = await ViewModel.AddUpdateLocationAsync(Location);

                messageDialog.HideLoading();
                if (isSuccess)
                {
                    messageDialog.SendToast("Location is saved successful.");
                    var intent = new Intent(this, typeof(FacilityDetailActivity));
                    Context mContext = Android.App.Application.Context;
                    AppPreferences ap = new AppPreferences(mContext);
                    ap.SaveFacilityId(Facility.Id.ToString());
                    Facility.Buildings = new List<Building>();
                    Facility.Location = Location;
                    intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(Facility));
                    this.StartActivity(intent);
                    Finish();
                }
                else
                {
                    messageDialog.SendToast("Error occurred: Unable to save location.");
                }
            }
        }

        private void RefashAccuracy_Click(object sender, EventArgs e)
        {
            GPSTracker GPSTracker = new GPSTracker();
            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (location != null)
            {
                accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
            }
        }
        private bool ValidateLocation()
        {
            Validations validation = new Validations();
            MessageDialog messageDialog = new MessageDialog();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            bool isValid = true;

            if (!validation.IsRequired(streetAddress.Text))
            {
                streetAddress.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(suburb.Text))
            {
                suburb.SetError("This field is required", icon);
                isValid = false;
            }
            if (Location == null)
            {
                if (Location.GPSCoordinates == null)
                {
                    messageDialog.SendToast("Please add an GPS coordinates");
                    isValid = false;
                }
            }
            else {
                if (Location.GPSCoordinates == null)
                {
                    messageDialog.SendToast("Please add an GPS coordinates");
                    isValid = false;
                }
            }
            return isValid;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            for (int j = 0; j < menu.Size(); j++)
            {
                var item = menu.GetItem(j);
                if (item.ToString() == "Search")
                    item.SetVisible(false);
                if (item.ToString() == "Submit")
                    item.SetVisible(false);
                if (item.ToString() == "Add")
                    item.SetVisible(false);
                if (item.ToString() == "Save")
                    item.SetShowAsActionFlags(Android.Views.ShowAsAction.Always);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }
        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
        }
        #endregion #endregion 
    }
}