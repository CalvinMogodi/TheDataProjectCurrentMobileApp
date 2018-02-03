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

namespace TheDataProject.Droid.Fragments
{
    public class FacilityInformationFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {


        #region Properties 
        public string Photo { get; set; }
        public static readonly int PickImageId = 1000;
        FloatingActionButton editButton, saveButton, gpsLocationButton, bpLocationButton;
        Button locationCancelButton, locationDoneButton, responsiblePersonCancelButton, responsiblePersonDoneButton, deedCancelButton, deedDoneButton;
        ProgressBar progress;        
        EditText streetAddress, suburb, fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        LayoutInflater Inflater;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        TextView clientCode, facilityName;
        ImageView facilityPhoto;
        LinearLayout locationlinearlayout;
        View view;
        ListView mListView;

        public static FacilityDetailViewModel ViewModel { get; set; }
        public static FacilityInformationFragment NewInstance() => new FacilityInformationFragment { Arguments = new Bundle() };

        #endregion #endregion 
         
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilityDetailViewModel();
            Inflater = inflater;
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
            facilityPhoto = view.FindViewById<ImageView>(Resource.Id.imgf_facilityphoto);

            // set data
            clientCode.Text = "Facility Name";
            facilityName.Text = "9558744000024800";
            
            //set settlement type drop down
            //var settlementTypeAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.settlementtypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            //settlementTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            //settlementtype.Adapter = settlementTypeAdapter;

            //set zoning drop down
            var zoningAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.zoningtypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            zoningAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            zoning.Adapter = zoningAdapter;
            
            saveButton.Visibility = ViewStates.Gone;
            editButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;
            locationHolder.Click += Location_Click;
            responsiblepersonHolder.Click += ResponsiblePerson_Click;
            deedHolder.Click += Deed_Click;
            facilityPhoto.Click += FacilityPhoto_Click;
           

            return view;
        }

        private void FacilityPhoto_Click(object sender, EventArgs eventArgs)
        {
            Intent Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }


        public void BecameVisible()
        {

        }

        void EditButton_Click(object sender, EventArgs e)
        {
            editButton.Visibility = ViewStates.Gone;
            saveButton.Visibility = ViewStates.Visible;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            editButton.Visibility = ViewStates.Visible;
            saveButton.Visibility = ViewStates.Gone;
        }

        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
        }

        #region Location 
        private void InitializeLocation(AlertDialog dialog)
        {
            streetAddress = dialog.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = dialog.FindViewById<EditText>(Resource.Id.etf_suburb);
            province = dialog.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = dialog.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            locationCancelButton = dialog.FindViewById<Button>(Resource.Id.dfil_cancelbutton);
            locationDoneButton = dialog.FindViewById<Button>(Resource.Id.dfil_donebutton);
            gpsLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.gpscaddlocation_button);
            bpLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.bpaddlocation_button);
            locationlinearlayout = dialog.FindViewById<LinearLayout>(Resource.Id.location_linearlayout);
            locationlinearlayout.Visibility = ViewStates.Gone;
                       
            
            //set province drop down
            var provinceAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.provinces, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            provinceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            province.Adapter = provinceAdapter;

            //set local municipality drop down
            var localmunicipalityAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.localmunicipalities, Android.Resource.Layout.SimpleDropDownItem1Line);
            localmunicipalityAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipality.Adapter = localmunicipalityAdapter;

            locationCancelButton.Click += LocationCancelButton_Click;
            locationDoneButton.Click += LocationDoneButton_Click;
            bpLocationButton.Click += BPLocationButton_Click;
            gpsLocationButton.Click += GPSLocationButton_Click;
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
           
        }

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
        {
            locationlinearlayout.Visibility = ViewStates.Visible;
            //GPSTracker GPSTracker = new GPSTracker();
            //Context context = view.Context;
            //MainActivity mainActivity = (MainActivity) context;

            LocationManager locationManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;
            
            Location location = null;
            // getting GPS status

            bool isGPSEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            bool isNetworkEnabled = locationManager.IsProviderEnabled(LocationManager.NetworkProvider);
            bool isPassiveProviderEnabled = locationManager.IsProviderEnabled(LocationManager.PassiveProvider);

            if (!isGPSEnabled && !isNetworkEnabled && !isPassiveProviderEnabled)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("GPS is not enabled");
                ShowSettingsAlert();
            }
            else
            {
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

            responsiblePersonCancelButton.Click += ResponsiblePersonCancelButton_Click;
            responsiblePersonDoneButton.Click += ResponsiblePersonDoneButton_Click;
        }

        private void ResponsiblePersonCancelButton_Click(object sender, EventArgs e)
        {
            responsiblePersonDialog.Cancel();
        }

        private void ResponsiblePersonDoneButton_Click(object sender, EventArgs e)
        {
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
            erfNumber = dialog.FindViewById<EditText>(Resource.Id.etf_fullname);
            titleDeedNumber = dialog.FindViewById<EditText>(Resource.Id.etf_designation);
            extentm2 = dialog.FindViewById<EditText>(Resource.Id.etf_mobileNumber);
            ownerInformation = dialog.FindViewById<EditText>(Resource.Id.etf_emailaddress);
            deedCancelButton = dialog.FindViewById<Button>(Resource.Id.dfid_cancelbutton);
            deedDoneButton = dialog.FindViewById<Button>(Resource.Id.dfid_donebutton);

            deedCancelButton.Click += DeedCancelButton_Click;
            deedDoneButton.Click += DeedDoneButton_Click;
        }

        private void DeedCancelButton_Click(object sender, EventArgs e)
        {
            deedDialog.Cancel();
        }

        private void DeedDoneButton_Click(object sender, EventArgs e)
        {
            deedDialog.Cancel();
        }
        #endregion #endregion        
    }
}