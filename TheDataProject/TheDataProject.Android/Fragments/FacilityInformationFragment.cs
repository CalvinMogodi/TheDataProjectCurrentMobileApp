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

namespace TheDataProject.Droid.Fragments
{
    public class FacilityInformationFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {


        #region Properties 
        public string Photo { get; set; }
        public static readonly int PickImageId = 1000;
        FloatingActionButton editButton, saveButton;
        Button locationCancelButton, locationDoneButton, responsiblePersonCancelButton, responsiblePersonDoneButton, deedCancelButton, deedDoneButton;
        ProgressBar progress;        
        EditText clientCode, facilityName, streetAddress, suburb, fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, polygontype, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        LayoutInflater Inflater;
        TextView locationHolder, responsiblepersonHolder, deedHolder;
        ImageView facilityPhoto;

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
            View view = inflater.Inflate(Resource.Layout.fragment_facility_information, container, false);
            editButton = view.FindViewById<FloatingActionButton>(Resource.Id.editfacilityinfo_button);
            saveButton = view.FindViewById<FloatingActionButton>(Resource.Id.savefacilityinfo_button);
            clientCode = view.FindViewById<EditText>(Resource.Id.etf_clientcode);
            facilityName = view.FindViewById<EditText>(Resource.Id.etf_facilityname);
            settlementtype = view.FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            zoning = view.FindViewById<Spinner>(Resource.Id.sf_zoning);
            locationHolder = view.FindViewById<TextView>(Resource.Id.tvf_locationholder);
            responsiblepersonHolder = view.FindViewById<TextView>(Resource.Id.tvf_responsiblepersonholder);
            deedHolder = view.FindViewById<TextView>(Resource.Id.tvf_deedholder);
            facilityPhoto = view.FindViewById<ImageView>(Resource.Id.imgf_facilityphoto);

            //set settlement type drop down
            var settlementTypeAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.settlementtypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            settlementTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            settlementtype.Adapter = settlementTypeAdapter;

            //set zoning drop down
            var zoningAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.zoningtypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            zoningAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            settlementtype.Adapter = zoningAdapter;
            
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

        #region Location 
        private void InitializeFacilityInfo(AlertDialog dialog)
        {
            streetAddress = dialog.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = dialog.FindViewById<EditText>(Resource.Id.etf_suburb);
            province = dialog.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = dialog.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            polygontype = dialog.FindViewById<Spinner>(Resource.Id.sf_polygontype);
            locationCancelButton = dialog.FindViewById<Button>(Resource.Id.dfil_cancelbutton);
            locationDoneButton = dialog.FindViewById<Button>(Resource.Id.dfil_donebutton);

            //set province drop down
            var provinceAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.provinces, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            provinceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            province.Adapter = provinceAdapter;

            //set local municipality drop down
            var localmunicipalityAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.localmunicipalities, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipalityAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipality.Adapter = localmunicipalityAdapter;

            //set polygon type drop down
            var polygontypeAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.polygontypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            polygontypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            polygontype.Adapter = polygontypeAdapter;

            locationCancelButton.Click += LocationCancelButton_Click;
            locationDoneButton.Click += LocationDoneButton_Click;
        }

        
        private void Location_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_location, null));
            locationDialog = builder.Create();

            locationDialog.Show();
            locationDialog.SetCanceledOnTouchOutside(false);
            InitializeFacilityInfo(locationDialog);
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