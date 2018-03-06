using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.Droid.Helpers;
using System.IO;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Locations;
using Android.Provider;
using Android.Content.PM;
using Android.Graphics.Drawables;
using TheDataProject.Models;
using static Android.Widget.AdapterView;
using TheDataProject.ViewModels;
using TheDataProject.Droid.Activities;

namespace TheDataProject.Droid.Fragments
{
    public class FacilityInformationFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {


        #region Properties 
        public string Photo { get; set; }
        public static readonly int PickImageId = 1000;
        FloatingActionButton editButton, saveButton, gpsLocationButton, bpLocationButton, refashAccuracy;
        Button locationCancelButton, locationDoneButton, responsiblePersonCancelButton, responsiblePersonDoneButton, deedCancelButton, deedDoneButton
            , takeaphotoButton, selectPictureButton, siCancelButton, siDoneButton;
        ProgressBar progress;
        EditText streetAddress, suburb, region, fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        LayoutInflater Inflater;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        ImageView pictureHolder;
        TextView clientCode, facilityName, tvfLatitude, tvfLongitude, boundaryPolygonsText, accuracyMessage;
        List<string> imageNames;
        View view;
        ViewGroup Container;
        ListView bpListView;
        List<string> itemList;
        Dialog imageDialog;
        FacilityDetailViewModel facilityDetailViewModel;
        public GPSCoordinate _GPSCoordinates;
        public List<BoundryPolygon> _BoundryPolygons;
        ArrayAdapter<string> arrayAdapter;
        int oldPosition;
        public bool isEdit = false;
        LocationManager _locationManager;
        string _locationProvider;
        public static FacilitiesViewModel ViewModel { get; set; }
        public Facility facility;
        public static FacilityInformationFragment NewInstance(Bundle mybundle) =>
            new FacilityInformationFragment { Arguments = mybundle };


        #endregion #endregion 

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();

            Inflater = inflater;
            Container = container;
            view = inflater.Inflate(Resource.Layout.fragment_facility_information, container, false);
            editButton = view.FindViewById<FloatingActionButton>(Resource.Id.editfacilityinfo_button);
            saveButton = view.FindViewById<FloatingActionButton>(Resource.Id.savefacilityinfo_button);
            clientCode = view.FindViewById<TextView>(Resource.Id.tvf_clientcode);
            facilityName = view.FindViewById<TextView>(Resource.Id.tvf_facilityname);
            settlementtype = view.FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            zoning = view.FindViewById<Spinner>(Resource.Id.sf_zoning);
            locationHolder = view.FindViewById<CardView>(Resource.Id.tvf_locationholder);
            responsiblepersonHolder = view.FindViewById<CardView>(Resource.Id.tvf_responsiblepersonholder);
            deedHolder = view.FindViewById<CardView>(Resource.Id.tvf_deedholder);
            pictureHolder = view.FindViewById<ImageView>(Resource.Id.facilityphotoimageinfo);
           
            facility = new Facility();

            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
           
            var data = Arguments.GetString("data");

            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                clientCode.Text = facility.ClientCode;
                facilityName.Text = facility.Name;
                settlementtype.SetSelection(GetIndex(settlementtype, facility.SettlementType));
                zoning.SetSelection(GetIndex(zoning, facility.Zoning));
                imageNames = facility.IDPicture == null ? new List<string>() : facility.IDPicture.Split(',').ToList();
                if (imageNames.Count > 0)
                    GetImages(ap);
            }
            pictureHolder.Click += (sender, e) => {
                var intent = new Intent(Activity, typeof(FacilityPictureActivity));
                intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
                StartActivity(intent);
            };
            settlementtype.Enabled = false;
            zoning.Enabled = false;
            saveButton.Visibility = ViewStates.Gone;
            editButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;
            locationHolder.Click += Location_Click;
            responsiblepersonHolder.Click += ResponsiblePerson_Click;
            deedHolder.Click += Deed_Click;
            _GPSCoordinates = new GPSCoordinate();
            _BoundryPolygons = new List<BoundryPolygon>();
            return view;
        }
        private async void GetImages(AppPreferences ap)
        {
            if (!String.IsNullOrEmpty(imageNames[0]))
            {
                Bitmap bit = ap.SetImageBitmap(ap.CreateDirectoryForPictures() + "/" + imageNames[0]);
                if (bit != null)
                    pictureHolder.SetImageBitmap(bit);
                else if (bit == null && !String.IsNullOrEmpty(imageNames[0]))
                {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Models.Picture picture = await pictureViewModel.ExecuteGetPictureCommand(imageNames[0]);
                    if (picture != null)
                    {
                        var _bit = ap.StringToBitMap(picture.File);
                        if (_bit != null)
                            ap.SaveImage(_bit, imageNames[0]);
                        pictureHolder.SetImageBitmap(_bit);
                    }
                }
            }            
        }
        private int GetIndex(Spinner spinner, String myString)
        {
            int index = 0;
            for (int i = 0; i < spinner.Count; i++)
            {
                if (spinner.GetItemAtPosition(i).Equals(myString))
                {
                    index = i;
                }
            }
            return index;
        }
        public void BecameVisible()
        {

        }

        void EditButton_Click(object sender, EventArgs e)
        {
            editButton.Visibility = ViewStates.Gone;
            saveButton.Visibility = ViewStates.Visible;
            isEdit = true;
            settlementtype.Enabled = true;
            zoning.Enabled = true;
        }

        async void SaveButton_Click(object sender, EventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();
            facility.SettlementType = settlementtype.SelectedItem.ToString();
            facility.Zoning = zoning.SelectedItem.ToString(); 
            bool isUpdated = await ViewModel.ExecuteUpdateFacilityCommand(facility);
            if (isUpdated)
            {
                editButton.Visibility = ViewStates.Visible;
                saveButton.Visibility = ViewStates.Gone;
                isEdit = false;
                settlementtype.Enabled = false;
                zoning.Enabled = false;
                messageDialog.HideLoading();
                messageDialog.SendToast("Facility is updated successful.");
            }
            else
            {
                messageDialog.HideLoading();
                messageDialog.SendToast("Facility is not updated successful.");
            }
            this.isEdit = false;
        }

        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
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
            if (facility.Location == null)
            {
                if (facility.Location.GPSCoordinates == null)
                {
                    messageDialog.SendToast("Please add an GPS coordinates");
                    isValid = false;
                }

            }
            return isValid;
        }



        private bool ValidateDeedInfo()
        {
            Validations validation = new Validations();
            MessageDialog messageDialog = new MessageDialog();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            bool isValid = true;

            if (!validation.IsRequired(erfNumber.Text))
            {
                erfNumber.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(titleDeedNumber.Text))
            {
                titleDeedNumber.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(extentm2.Text))
            {
                extentm2.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(ownerInformation.Text))
            {
                ownerInformation.SetError("This field is required", icon);
                isValid = false;
            }
            return isValid;
        }

        #region Location 
        private void InitializeLocation(AlertDialog dialog)
        {
            int listViewMinimumHeight = 25;
               streetAddress = dialog.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = dialog.FindViewById<EditText>(Resource.Id.etf_suburb);
            region = dialog.FindViewById<EditText>(Resource.Id.etf_region);
            province = dialog.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = dialog.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            locationCancelButton = dialog.FindViewById<Button>(Resource.Id.dfil_cancelbutton);
            locationDoneButton = dialog.FindViewById<Button>(Resource.Id.dfil_donebutton);
            gpsLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.gpscaddlocation_button);
            bpLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.bpaddlocation_button);
            refashAccuracy = dialog.FindViewById<FloatingActionButton>(Resource.Id.refreshaccuracy_button);
            tvfLatitude = dialog.FindViewById<TextView>(Resource.Id.tvf_latitude);
            tvfLongitude = dialog.FindViewById<TextView>(Resource.Id.tvf_longitude);
            boundaryPolygonsText = dialog.FindViewById<TextView>(Resource.Id.boundaryPolygonsText);
            accuracyMessage = dialog.FindViewById<TextView>(Resource.Id.accuracy_message);
            bpListView = dialog.FindViewById<ListView>(Resource.Id.bplistView1);
            itemList = new List<string>();
            bpListView.SetMinimumHeight(listViewMinimumHeight);
            refashAccuracy.Click += RefashAccuracy_Click;
            if (facility.Location != null)
            {
                streetAddress.Text = facility.Location.StreetAddress;
                suburb.Text = facility.Location.Suburb;
                region.Text = facility.Location.Region;
                province.SetSelection(GetIndex(province, facility.Location.Province));
                localmunicipality.SetSelection(GetIndex(localmunicipality, facility.Location.LocalMunicipality));
                listViewMinimumHeight = listViewMinimumHeight * facility.Location.BoundryPolygon.Count();
                if (facility.Location.GPSCoordinates != null)
                {
                    tvfLatitude.Text = "Lat: " + facility.Location.GPSCoordinates.Latitude;
                    tvfLongitude.Text = " Long: " + facility.Location.GPSCoordinates.Longitude;
                }

                
                if (facility.Location.BoundryPolygon != null)
                {
                    bpListView.SetMinimumHeight(listViewMinimumHeight);
                    _BoundryPolygons = new List<BoundryPolygon>();
                    foreach (var BoundaryPolygon in facility.Location.BoundryPolygon)
                    {
                        _BoundryPolygons.Add(BoundaryPolygon);
                        itemList.Add("Lat: " + BoundaryPolygon.Latitude.ToString() + " Long: " + BoundaryPolygon.Longitude.ToString());
                    }

                    arrayAdapter = new ArrayAdapter<string>(Activity, Resource.Layout.list_item, itemList);
                    bpListView.Adapter = arrayAdapter;
                    bpListView.ItemLongClick += Adapter_ItemSwipe;
                }
                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
            }


            locationCancelButton.Click += LocationCancelButton_Click;
            locationDoneButton.Click += LocationDoneButton_Click;
            bpLocationButton.Click += BPLocationButton_Click;
            gpsLocationButton.Click += GPSLocationButton_Click;

            Android.Content.Res.ColorStateList csl = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor("#008000") }); bpLocationButton.BackgroundTintList = csl;
            Android.Content.Res.ColorStateList cslf = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor("#008000") }); gpsLocationButton.BackgroundTintList = cslf;

            if (isEdit)
            {
                streetAddress.Enabled = true;
                suburb.Enabled = true;
                region.Enabled = true;
                province.Enabled = true;
                localmunicipality.Enabled = true;
                locationDoneButton.Enabled = true;
                gpsLocationButton.Enabled = true;
                bpLocationButton.Enabled = true;
            }
            else
            {
                streetAddress.Enabled = false;
                suburb.Enabled = false;
                region.Enabled = false;
                province.Enabled = false;
                localmunicipality.Enabled = false;
                locationDoneButton.Enabled = false;
                gpsLocationButton.Enabled = false;
                bpLocationButton.Enabled = false;
            }

            GPSTracker GPSTracker = new GPSTracker();
            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (location != null)
            {
                accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
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

        private void Location_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_location, null));
            locationDialog = builder.Create();

            locationDialog.Show();
            locationDialog.SetCanceledOnTouchOutside(false);
            InitializeLocation(locationDialog);
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
            else {
                BoundryPolygon BoundryPolygon = new BoundryPolygon()
                {
                    Latitude = location.Latitude.ToString(),
                    Longitude = location.Longitude.ToString()
                };
                _BoundryPolygons.Add(BoundryPolygon);
                itemList.Add("Lat: " + location.Latitude.ToString() + " Long: " + location.Longitude.ToString());

                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                arrayAdapter = new ArrayAdapter<string>(Activity, Resource.Layout.list_item, itemList);
                bpListView.Adapter = arrayAdapter;
                bpListView.ItemLongClick += Adapter_ItemSwipe;
                int listViewMinimumHeight = 25 * _BoundryPolygons.Count();
                bpListView.SetMinimumHeight(listViewMinimumHeight);
            }
           
        }
        void Adapter_ItemSwipe(object sender, ItemLongClickEventArgs e)
        {
            if (_BoundryPolygons.Count() > 0)
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

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
        {
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
            tvfLatitude.Text = "Lat: " + location.Latitude.ToString();
            tvfLongitude.Text = " Long: " + location.Longitude.ToString();
            accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
        }
        private void LocationCancelButton_Click(object sender, EventArgs e)
        {
            locationDialog.Cancel();
        }

        private void LocationDoneButton_Click(object sender, EventArgs e)
        {
            if (!ValidateLocation())
                return;

            facility.Location = new Models.Location();
            facility.Location.LocalMunicipality = localmunicipality.SelectedItem.ToString();
            facility.Location.Province = province.SelectedItem.ToString();
            facility.Location.StreetAddress = streetAddress.Text;
            facility.Location.Suburb = suburb.Text;
            facility.Location.Region = region.Text;
            facility.Location.GPSCoordinates = new Models.GPSCoordinate()
            {
                Longitude = tvfLongitude.Text.Substring(6),                
                Latitude = tvfLatitude.Text.Substring(5),
            };
            facility.Location.BoundryPolygon = _BoundryPolygons;
            locationDialog.Cancel();
        }
        #endregion #endregion 

        #region Responsible Person 

        private void ResponsiblePerson_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_responsible_person, null));
            responsiblePersonDialog = builder.Create();

            responsiblePersonDialog.Show();
            responsiblePersonDialog.SetCanceledOnTouchOutside(false);
            InitializeResponsiblePerson(responsiblePersonDialog);
        }

        private void InitializeResponsiblePerson(AlertDialog dialog)
        {
            fullname = dialog.FindViewById<EditText>(Resource.Id.etf_fullname);
            designation = dialog.FindViewById<EditText>(Resource.Id.etf_designation);
            mobileNumber = dialog.FindViewById<EditText>(Resource.Id.etf_mobileNumber);
            emailaddress = dialog.FindViewById<EditText>(Resource.Id.etf_emailaddress);
            responsiblePersonCancelButton = dialog.FindViewById<Button>(Resource.Id.dfirp_cancelbutton);
            responsiblePersonDoneButton = dialog.FindViewById<Button>(Resource.Id.dfirp_donebutton);

            if (facility.ResposiblePerson != null)
            {
                fullname.Text = facility.ResposiblePerson.FullName;
                designation.Text = facility.ResposiblePerson.Designation;
                mobileNumber.Text = facility.ResposiblePerson.PhoneNumber;
                emailaddress.Text = facility.ResposiblePerson.EmailAddress;
            }

            responsiblePersonCancelButton.Click += ResponsiblePersonCancelButton_Click;
            responsiblePersonDoneButton.Click += ResponsiblePersonDoneButton_Click;

            if (isEdit)
            {
                fullname.Enabled = true;
                designation.Enabled = true;
                mobileNumber.Enabled = true;
                emailaddress.Enabled = true;
                responsiblePersonDoneButton.Enabled = true;
            }
            else
            {
                fullname.Enabled = false;
                designation.Enabled = false;
                mobileNumber.Enabled = false;
                emailaddress.Enabled = false;
                responsiblePersonDoneButton.Enabled = false;
            }
        }

        private void ResponsiblePersonCancelButton_Click(object sender, EventArgs e)
        {
            responsiblePersonDialog.Cancel();
        }

        private void ResponsiblePersonDoneButton_Click(object sender, EventArgs e)
        {

            facility.ResposiblePerson = new Models.Person();
            facility.ResposiblePerson.FullName = fullname.Text;
            facility.ResposiblePerson.Designation = designation.Text;
            facility.ResposiblePerson.PhoneNumber = mobileNumber.Text;
            facility.ResposiblePerson.EmailAddress = emailaddress.Text;
            responsiblePersonDialog.Cancel();
        }
        #endregion #endregion 

        #region Deed Office

        private void Deed_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_deed, null));
            deedDialog = builder.Create();

            deedDialog.Show();
            deedDialog.SetCanceledOnTouchOutside(false);
            InitializeDeed(deedDialog);
        }

        private void InitializeDeed(AlertDialog dialog)
        {
            erfNumber = dialog.FindViewById<EditText>(Resource.Id.etf_erfnumber);
            titleDeedNumber = dialog.FindViewById<EditText>(Resource.Id.etf_titledeednumber);
            extentm2 = dialog.FindViewById<EditText>(Resource.Id.etf_extentm2);
            ownerInformation = dialog.FindViewById<EditText>(Resource.Id.etf_ownerinformation);
            deedCancelButton = dialog.FindViewById<Button>(Resource.Id.dfid_cancelbutton);
            deedDoneButton = dialog.FindViewById<Button>(Resource.Id.dfid_donebutton);

            if (facility.DeedsInfo != null)
            {
                erfNumber.Text = facility.DeedsInfo.ErFNumber;
                titleDeedNumber.Text = facility.DeedsInfo.TitleDeedNumber;
                extentm2.Text = facility.DeedsInfo.Extent;
                ownerInformation.Text = facility.DeedsInfo.OwnerInfomation;

            }
            deedCancelButton.Click += DeedCancelButton_Click;
            deedDoneButton.Click += DeedDoneButton_Click;

            if (isEdit)
            {
                erfNumber.Enabled = true;
                titleDeedNumber.Enabled = true;
                extentm2.Enabled = true;
                ownerInformation.Enabled = true;
                deedDoneButton.Enabled = true;
            }
            else
            {
                erfNumber.Enabled = false;
                titleDeedNumber.Enabled = false;
                extentm2.Enabled = false;
                ownerInformation.Enabled = false;
                deedDoneButton.Enabled = false;
            }
        }

        private void DeedCancelButton_Click(object sender, EventArgs e)
        {
            deedDialog.Cancel();
        }

        private void DeedDoneButton_Click(object sender, EventArgs e)
        {
            if (!ValidateDeedInfo())
                return;
            facility.DeedsInfo = new Models.DeedsInfo();
            facility.DeedsInfo.ErFNumber = erfNumber.Text;
            facility.DeedsInfo.TitleDeedNumber = titleDeedNumber.Text;
            facility.DeedsInfo.Extent = extentm2.Text;
            facility.DeedsInfo.OwnerInfomation = ownerInformation.Text;
            deedDialog.Cancel();
        }
        #endregion #endregion        
    }


}