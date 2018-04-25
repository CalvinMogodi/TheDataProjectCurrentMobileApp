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
        LayoutInflater Inflater;
        LinearLayout locationHolder, responsiblepersonHolder, deedHolder;
        ImageView pictureHolder;
        TextView clientCode, facilityName;
        List<string> imageNames;
        View view;
        ViewGroup Container;
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
            clientCode = view.FindViewById<TextView>(Resource.Id.tvf_clientcode);
            facilityName = view.FindViewById<TextView>(Resource.Id.tvf_facilityname);          
            locationHolder = view.FindViewById<LinearLayout>(Resource.Id.tvf_locationholder);
            responsiblepersonHolder = view.FindViewById<LinearLayout>(Resource.Id.tvf_responsiblepersonholder);
            deedHolder = view.FindViewById<LinearLayout>(Resource.Id.tvf_deedholder);
            pictureHolder = view.FindViewById<ImageView>(Resource.Id.facilityphotoimageinfo);
            facility = new Facility();

            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
           
            var data = Arguments.GetString("data");

            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                clientCode.Text = facility.ClientCode;
                facilityName.Text = facility.Name;
                imageNames = facility.IDPicture == null ? new List<string>() : facility.IDPicture.Split(',').ToList();
                if (imageNames.Count > 0)
                    GetImages(ap);
            }
            pictureHolder.Click += (sender, e) => {
                var intent = new Intent(Activity, typeof(FacilityPictureActivity));
                intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
                StartActivity(intent);
            };           
            locationHolder.Click += Location_Click;
            responsiblepersonHolder.Click += ResponsiblePerson_Click;
            deedHolder.Click += Deed_Click;
            HasOptionsMenu = true;
            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.top_menus, menu);
            base.OnCreateOptionsMenu(menu, inflater);
            for (int j = 0; j < menu.Size(); j++)
            {
                var item = menu.GetItem(j);
                if (item.ToString() == "Search")
                    item.SetVisible(false);
                if (item.ToString() == "Submit")
                    item.SetShowAsActionFlags(Android.Views.ShowAsAction.Always);
                if (item.ToString() == "Add")
                    item.SetVisible(false);

            }
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

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();
            BuildingsViewModel ViewModel = new BuildingsViewModel();
            await ViewModel.ExecuteBuildingsCommand(facility.Id);
           // var buildings = ViewModel.Buildings;

            //if (!ValidateForm(facility, buildings, messageDialog))
            //{
            //    messageDialog.HideLoading();
            //    return;
            //}

            //facility.Status = "Submitted";
            //facility.ModifiedUserId = userId;
            //facility.ModifiedDate = new DateTime();
            //bool isUpdated = await viewModel.ExecuteUpdateFacilityCommand(facility);
            //messageDialog.HideLoading();
            //if (isUpdated)
            //{
            //    viewModel.Facilities.Remove(viewModel.Facilities.Where(s => s.Id == facility.Id).Single());
            //    messageDialog.SendToast("Facility is submitted for approval.");
            //    var myActivity = (MainActivity)this.activity;
            //    myActivity.Recreate();
            //}
            //else
            //{
            //    messageDialog.SendToast("Unable to submitted facility for approval.");
            //}
        }

        private bool ValidateForm(Facility facility, MessageDialog messageDialog)
        {
            Validations validation = new Validations();

            bool isValid = true;
            bool deedsInfoIsRequired = false;
            bool locationfoIsRequired = false;
            bool pictureIsRequired = false;
            bool buildingPictureIsRequired = false;
            bool buildingLocationIsRequired = false;

            //foreach (var building in buildings)
            //{
            //    if (String.IsNullOrEmpty(building.Photo))
            //    {
            //        buildingPictureIsRequired = true;
            //        isValid = false;
            //    }
            //    if (building.GPSCoordinates == null)
            //    {
            //        buildingLocationIsRequired = true;
            //        isValid = false;
            //    }
            //}

            if (facility.DeedsInfo == null)
            {
                deedsInfoIsRequired = true;
                isValid = false;
            }
            if (facility.Location == null)
            {
                locationfoIsRequired = true;
                isValid = false;
            }

            if (!validation.IsRequired(facility.IDPicture))
            {
                pictureIsRequired = true;
                isValid = false;
            }

            if (deedsInfoIsRequired || locationfoIsRequired || pictureIsRequired || buildingLocationIsRequired || buildingPictureIsRequired)
            {
                if (deedsInfoIsRequired && locationfoIsRequired && pictureIsRequired)
                    messageDialog.SendToast("Please add an image, location information and deeds information");
                else if (deedsInfoIsRequired)
                    messageDialog.SendToast("Please capture deeds information.");
                else if (locationfoIsRequired)
                    messageDialog.SendToast("Please capture location information.");
                else if (pictureIsRequired)
                    messageDialog.SendToast("Please add an image.");
                else if (buildingPictureIsRequired && buildingLocationIsRequired)
                    messageDialog.SendToast("Please add an image and location for all the buildings.");
                else if (buildingPictureIsRequired)
                    messageDialog.SendToast("Please add an image for all the buildings.");
                else if (buildingLocationIsRequired)
                    messageDialog.SendToast("Please add location for all the buildings.");
            }
            return isValid;
        }

        private void Information_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(LocationActivity));
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
            StartActivity(intent);
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