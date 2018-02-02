using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TheDataProject.ViewModels;
using TheDataProject.Models;
using Android.Support.Design.Widget;
using Android.Content.PM;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "BuildingDetailsActivity", LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class BuildingDetailsActivity : BaseActivity
    {

        protected override int LayoutResource => Resource.Layout.activity_building_details;

        BuildingDetailViewModel viewModel;
        FloatingActionButton editButton, saveButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var data = Intent.GetStringExtra("data");

            var item = Newtonsoft.Json.JsonConvert.DeserializeObject<Building>(data);
            viewModel = new BuildingDetailViewModel(item);

            FindViewById<TextView>(Resource.Id.description).Text = item.BuildingNumber;
            editButton = FindViewById<FloatingActionButton>(Resource.Id.editbuildinginfo_button);
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.savebuildinginfo_button);

            saveButton.Visibility = ViewStates.Gone;
            editButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;

            SupportActionBar.Title = item.Name;
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

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
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