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
        FloatingActionButton editButton, saveButton;        
        Spinner settlementtype, zoning;
        LayoutInflater Inflater;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        ImageView pictureHolder;
        TextView clientCode, facilityName;
        List<string> imageNames;
        View view;
        ViewGroup Container;
        ListView bpListView;
        List<string> itemList;
        Dialog imageDialog;
        FacilityDetailViewModel facilityDetailViewModel;
        public bool isEdit = false;
        public static FacilitiesViewModel ViewModel { get; set; }
        public Facility facility;
        public static FacilityInformationFragment NewInstance(Bundle mybundle) => new FacilityInformationFragment { Arguments = mybundle };

        #endregion #endregion 

        #region Methods

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
       
        private void Location_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(LocationActivity));
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
            StartActivity(intent);
        }

        private void ResponsiblePerson_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(PersonActivity));
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
            StartActivity(intent);
        }

        private void Deed_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(DeedInforActivity));
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
            StartActivity(intent);
        }

        #endregion #endregion        
    }


}