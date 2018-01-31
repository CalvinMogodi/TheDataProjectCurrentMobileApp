using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.ViewModels;
using TheDataProject.Models;

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
            saveButton.Click += SaveButton_Click;
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            var item = new Building
            {
                Name = title.Text,
                Description = description.Text
            };
            ViewModel.AddBuildingCommand.Execute(item);

            Finish();
        }
    }
}
