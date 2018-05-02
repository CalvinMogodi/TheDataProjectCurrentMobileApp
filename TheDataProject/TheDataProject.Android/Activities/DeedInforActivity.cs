using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TheDataProject.Droid.Helpers;
using TheDataProject.Models;
using TheDataProject.ViewModels;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Deed Information", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DeedInforActivity : BaseActivity
    {
        #region Properties
        EditText erfNumber, titleDeedNumber, extentm2, ownerInformation;
        DeedsInfo DeedsInfo;
        Facility Facility;
        private int userId;
        public DeedInforViewModel ViewModel { get; set; }
        private AppPreferences appPreferences;
        protected override int LayoutResource => Resource.Layout.activity_deedInfor;

        #endregion #endregion 

        #region Methods 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            appPreferences = new AppPreferences(Application.Context);
            userId = Convert.ToInt32(appPreferences.GetUserId());
            erfNumber = FindViewById<EditText>(Resource.Id.etf_erfnumber);
            titleDeedNumber = FindViewById<EditText>(Resource.Id.etf_titledeednumber);
            extentm2 = FindViewById<EditText>(Resource.Id.etf_extentm2);
            ownerInformation = FindViewById<EditText>(Resource.Id.etf_ownerinformation);
            ViewModel = new DeedInforViewModel();
            DeedsInfo = new DeedsInfo();
            var data = Intent.GetStringExtra("data");
            if (data != null)
            {
                Facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                DeedsInfo = Facility.DeedsInfo;
                if (DeedsInfo != null)
                {
                    erfNumber.Text = DeedsInfo.ErFNumber;
                    titleDeedNumber.Text = DeedsInfo.TitleDeedNumber;
                    extentm2.Text = DeedsInfo.Extent;
                    ownerInformation.Text = DeedsInfo.OwnerInfomation;

                }
            }

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
                        SaveDeedInfor();
                        break;
                }
            };
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
        
        private async void SaveDeedInfor()
        {
            if (appPreferences.IsOnline(Application.Context))
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();

                if (!ValidateDeedInfo())
                {
                    messageDialog.HideLoading();
                    return;
                }
                DeedsInfo.ErFNumber = erfNumber.Text;
                DeedsInfo.TitleDeedNumber = titleDeedNumber.Text;
                DeedsInfo.Extent = extentm2.Text;
                DeedsInfo.OwnerInfomation = ownerInformation.Text;
                DeedsInfo.FacilityId = Facility.Id;
                DeedsInfo.CreatedUserId = userId;
                DeedsInfo.ModifiedDate = DateTime.Now;
                DeedsInfo.ModifiedUserId = userId;
                DeedsInfo.CreatedDate = DateTime.Now;
                bool isSuccess = await ViewModel.AddUpdateDeedsInfoAsync(DeedsInfo);

                messageDialog.HideLoading();
                if (isSuccess)
                {
                    messageDialog.SendToast("Deeds information is saved successful.");
                    var intent = new Intent(this, typeof(FacilityDetailActivity));
                    appPreferences.SaveFacilityId(Facility.Id.ToString());
                    Facility.Buildings = new List<Building>();
                    Facility.DeedsInfo = DeedsInfo;
                    intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(Facility));
                    this.StartActivity(intent);
                    Finish();
                }
                else
                {
                    messageDialog.SendToast("Error occurred: Unable to save deed information.");
                }
            }
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
        #endregion #endregion 
    }
}