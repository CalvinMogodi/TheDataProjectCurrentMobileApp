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
    [Activity(Label = "Responsible Person", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PersonActivity : BaseActivity
    {
        #region Properties
        private int userId;
        EditText fullname, designation, mobileNumber, emailaddress;
        Person Person;
        Facility Facility;
        public PersonViewModel ViewModel { get; set; }
        private AppPreferences appPreferences;
        protected override int LayoutResource => Resource.Layout.activity_person;
        #endregion #endregion 

        #region Methods 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            appPreferences = new AppPreferences(Application.Context);
            userId = Convert.ToInt32(appPreferences.GetUserId());
            fullname = FindViewById<EditText>(Resource.Id.etf_fullname);
            designation = FindViewById<EditText>(Resource.Id.etf_designation);
            mobileNumber = FindViewById<EditText>(Resource.Id.etf_mobileNumber);
            emailaddress = FindViewById<EditText>(Resource.Id.etf_emailaddress);
            ViewModel = new PersonViewModel();
            Person = new Person();
            var data = Intent.GetStringExtra("data");
            if (data != null)
            {
                Facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                Person = Facility.ResposiblePerson;
                if (Person != null)
                {
                    fullname.Text = Person.FullName;
                    designation.Text = Person.Designation;
                    mobileNumber.Text = Person.PhoneNumber;
                    emailaddress.Text = Person.EmailAddress;
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
                        SavePerson();
                        break;
                }
            };

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
        
        private async void SavePerson()
        {
            if (appPreferences.IsOnline(Application.Context))
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();

                Person.FullName = fullname.Text;
                Person.Designation = designation.Text;
                Person.PhoneNumber = mobileNumber.Text;
                Person.EmailAddress = emailaddress.Text;
                Person.FacilityId = Facility.Id;
                Person.CreatedUserId = userId;
                Person.ModifiedDate = DateTime.Now;
                Person.ModifiedUserId = userId;
                Person.CreatedDate = DateTime.Now;
                bool isSuccess = await ViewModel.AddUpdatePersonAsync(Person);

                messageDialog.HideLoading();
                if (isSuccess)
                {
                    messageDialog.SendToast("Responsible person is saved successful.");
                    var intent = new Intent(this, typeof(FacilityDetailActivity));
                    Context mContext = Android.App.Application.Context;
                    AppPreferences ap = new AppPreferences(mContext);
                    ap.SaveFacilityId(Facility.Id.ToString());
                    Facility.Buildings = new List<Building>();
                    Facility.ResposiblePerson = Person;
                    intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(Facility));
                    this.StartActivity(intent);
                    Finish();
                }
                else
                {
                    messageDialog.SendToast("Error occurred: Unable to save responsible person.");
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