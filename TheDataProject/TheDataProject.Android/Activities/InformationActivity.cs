using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheDataProject.Droid.Helpers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "InformationActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InformationActivity : BaseActivity
    {
        #region Properties
        Spinner settlementtype, zoning;
        private UIHelpers helpers;
        private AppPreferences appPreferences;
        Facility facility;
        FacilitiesViewModel viewModel;
        protected override int LayoutResource => Resource.Layout.activity_information;
        #endregion #endregion 

        #region Methods 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            helpers = new UIHelpers();
            appPreferences = new AppPreferences(Application.Context);
            settlementtype = FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            zoning = FindViewById<Spinner>(Resource.Id.sf_zoning);

            var data = Intent.GetStringExtra("data");
            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                if (String.IsNullOrEmpty(facility.SettlementType))
                    settlementtype.SetSelection(helpers.GetSpinnerIndex(settlementtype, facility.SettlementType));
                if(String.IsNullOrEmpty(facility.Zoning))
                    zoning.SetSelection(helpers.GetSpinnerIndex(zoning, facility.Zoning));
            }

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
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

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            facility.SettlementType = settlementtype.SelectedItem.ToString();
            facility.Zoning = zoning.SelectedItem.ToString();

            if (appPreferences.IsOnline(Application.Context))
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                bool isUpdated = await viewModel.ExecuteUpdateFacilityCommand(facility);
                messageDialog.HideLoading();
                if (isUpdated)
                {
                    messageDialog.SendToast("Facility Information is saved successful.");
                    var intent = new Intent(this, typeof(FacilityDetailActivity));
                    Context mContext = Android.App.Application.Context;
                    AppPreferences ap = new AppPreferences(mContext);
                    ap.SaveFacilityId(facility.Id.ToString());
                    intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
                    this.StartActivity(intent);
                    Finish();
                }
                else {
                    messageDialog.SendToast("Error occurred: Unable to save Facility Information.");
                }
            }            
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        #endregion #endregion 
    }
}