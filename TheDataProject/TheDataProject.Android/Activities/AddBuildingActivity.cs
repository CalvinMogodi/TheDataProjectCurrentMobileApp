using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.ViewModels;
using TheDataProject.Models;
using Android.Views;
using Android.Content.PM;

namespace TheDataProject.Droid
{
    [Activity(Label = "AddItemActivity", LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddBuildingActivity : BaseActivity
    {
        FloatingActionButton saveButton;
        EditText title, description, occupationYear;
        NumberPicker np;
        Button b1 , b2;
        Dialog d;
        TextInputLayout occupationyearLayout;
        public BuildingsViewModel ViewModel { get; set; }
        public bool isEdit = false;

        protected override int LayoutResource => Resource.Layout.activity_add_building;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel = new BuildingsViewModel();

            var data = Intent.GetStringExtra("data");

            if (data != null)
            {
                var item = Newtonsoft.Json.JsonConvert.DeserializeObject<Building>(data);
                isEdit = true;
                SupportActionBar.Title = "Edit Building";
            }
            else {
                SupportActionBar.Title = "Add New Building";
            }
                

                // Create your application here
                saveButton = FindViewById<FloatingActionButton>(Resource.Id.save_button);
            occupationYear = FindViewById<EditText>(Resource.Id.etb_occupationyear);
            occupationyearLayout = FindViewById<TextInputLayout>(Resource.Id.occupationyear_layout);
            occupationyearLayout.Click += (sender, e) => {
                show();
            };

            



            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.Click += SaveButton_Click;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        void OnOccupationYearSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            occupationYear.Text = e.Date.ToLongDateString();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            var item = new Building
            {
                Name = title.Text,
                BuildingNumber = description.Text
            };
            ViewModel.AddBuildingCommand.Execute(item);

            Finish();
        }
        void NumberPickerCancelButton_Click(object sender, EventArgs e)
        {          
            d.Dismiss();
        }

        void NumberPickerDoneButton_Click(object sender, EventArgs e)
        {
            occupationYear.Text = np.Value.ToString();
            d.Dismiss();
        }

        public void show()
        {

            d = new Dialog(this);
            d.SetTitle("Occupation Year");
            d.SetContentView(Resource.Layout.dialog_numberpicker);
            b1 = (Button)d.FindViewById(Resource.Id.button1);
            b2 = (Button)d.FindViewById(Resource.Id.button2);
            np = (NumberPicker)d.FindViewById(Resource.Id.numberPicker1);
            np.MaxValue = 1900;
            np.MinValue = 2200;
            np.WrapSelectorWheel = true;
            b1.Click += NumberPickerDoneButton_Click ;
            b2.Click += NumberPickerCancelButton_Click;
            d.Show();
        }
    }
}
