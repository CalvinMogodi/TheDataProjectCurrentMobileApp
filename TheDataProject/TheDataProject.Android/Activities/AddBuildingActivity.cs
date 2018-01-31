using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.ViewModels;
using TheDataProject.Models;
using Android.Views;

namespace TheDataProject.Droid
{
    [Activity(Label = "AddItemActivity")]
    public class AddBuildingActivity : BaseActivity
    {
        FloatingActionButton saveButton;
        EditText title, description;

        public BuildingsViewModel ViewModel { get; set; }

        protected override int LayoutResource => Resource.Layout.activity_add_building;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel = new BuildingsViewModel();

            // Create your application here
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.save_button);
            title = FindViewById<EditText>(Resource.Id.txtTitle);
            description = FindViewById<EditText>(Resource.Id.txtDesc);

            SupportActionBar.Title = "Add New Building";
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.Click += SaveButton_Click;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
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
    }
}
