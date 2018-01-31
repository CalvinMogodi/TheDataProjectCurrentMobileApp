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

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "BuildingDetailsActivity")]
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

            FindViewById<TextView>(Resource.Id.description).Text = item.Description;
            editButton = FindViewById<FloatingActionButton>(Resource.Id.editbuildinginfo_button);
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.savebuildinginfo_button);

            saveButton.Visibility = ViewStates.Gone;

            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;

            SupportActionBar.Title = item.Name;
            SupportActionBar.SetHomeButtonEnabled(true);
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