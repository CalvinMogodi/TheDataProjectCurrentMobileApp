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

namespace TheDataProject.Droid.Fragments
{
    public class FacilityInformationFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {
        public static FacilityInformationFragment NewInstance() => new FacilityInformationFragment { Arguments = new Bundle() };

        FloatingActionButton editButton, saveButton;
        ProgressBar progress;
        EditText clientCode, facilityName, streetAddress, suburb;
        Spinner settlementtype, province, localmunicipality, polygontype;

        public static FacilityDetailViewModel ViewModel { get; set; }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilityDetailViewModel();

            View view = inflater.Inflate(Resource.Layout.fragment_facility_information, container, false);
            editButton = view.FindViewById<FloatingActionButton>(Resource.Id.editfacilityinfo_button);
            saveButton = view.FindViewById<FloatingActionButton>(Resource.Id.savefacilityinfo_button);
            clientCode = view.FindViewById<EditText>(Resource.Id.etf_clientcode);
            facilityName = view.FindViewById<EditText>(Resource.Id.etf_facilityname);
            settlementtype = view.FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            streetAddress = view.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = view.FindViewById<EditText>(Resource.Id.etf_suburb);
            province = view.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = view.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            polygontype = view.FindViewById<Spinner>(Resource.Id.sf_polygontype);
            

            //set settlement type drop down
           // List<string> settlementtypelist = new List<string>();
            var adapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.settlementtypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            settlementtype.Adapter = adapter;

            //set province drop down
            List<string> provinceList = new List<string>();
            var provinceAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.provinces, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            provinceAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            province.Adapter = provinceAdapter;

            //set local municipality drop down
           // List<string> localmunicipalityList = new List<string>();
            var localmunicipalityAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.localmunicipalities, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipalityAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipality.Adapter = localmunicipalityAdapter;

            //set polygon type drop down
            //List<string> lpolygontypeList = new List<string>();
            var polygontypeAdapter = ArrayAdapter.CreateFromResource(Activity, Resource.Array.polygontypes, Android.Resource.Layout.SimpleSpinnerDropDownItem);
            localmunicipalityAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            polygontype.Adapter = polygontypeAdapter;

            saveButton.Visibility = ViewStates.Gone;
            editButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;
            return view;
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
    }
}