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

        Button cancelButton, saveButton;
        EditText fullname, designation, mobileNumber, emailaddress;
        Person Person;
        Facility Facility;
        public PersonViewModel ViewModel { get; set; }
        protected override int LayoutResource => Resource.Layout.activity_person;

        #endregion #endregion 

        #region Methods 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            fullname = FindViewById<EditText>(Resource.Id.etf_fullname);
            designation = FindViewById<EditText>(Resource.Id.etf_designation);
            mobileNumber = FindViewById<EditText>(Resource.Id.etf_mobileNumber);
            emailaddress = FindViewById<EditText>(Resource.Id.etf_emailaddress);
            cancelButton = FindViewById<Button>(Resource.Id.dfirp_cancelbutton);
            saveButton = FindViewById<Button>(Resource.Id.dfirp_donebutton);
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

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            cancelButton.Click += CancelButton_Click;
            saveButton.Click += SaveButton_Click;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
           Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();

            Person.FullName = fullname.Text;
            Person.Designation = designation.Text;
            Person.PhoneNumber = mobileNumber.Text;
            Person.EmailAddress = emailaddress.Text;
            bool isSuccess = true;// await ViewModel.AddUpdatePersonAsync(Person);

            messageDialog.HideLoading();
            if (isSuccess){
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
            else {
                messageDialog.SendToast("Error occurred: Unable to save responsible person.");
            }
        }

        #endregion #endregion 
    }
}