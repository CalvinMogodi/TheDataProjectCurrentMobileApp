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
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using TheDataProject.Droid.Helpers;
using Android.Graphics;
using TheDataProject.ViewModels;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "BuildingsActivity")]
    public class BuildingsActivity : BaseActivity
    {
        protected override int LayoutResource => Resource.Layout.activity_buildings;
        
        Button informationButton;
        BrowseBuildingsAdapter adapter;
        SwipeRefreshLayout refresher;
        FloatingActionButton addButton;
        RecyclerView recyclerView;
        ProgressBar progress;
        int facilityId;
        Facility facility;
        public static BuildingsViewModel ViewModel { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ViewModel = new BuildingsViewModel();
            
            AppPreferences ap = new AppPreferences(Application.Context);
            facilityId = Convert.ToInt32(ap.GetFacilityId());
            var data = Intent.GetStringExtra("data");

            informationButton = FindViewById<Button>(Resource.Id.information_button);
            informationButton.Touch += (sender, e) =>
            {
                var intent = new Intent(this, typeof(FacilityInformationActivity));
                StartActivity(intent);
            };

            recyclerView = FindViewById<RecyclerView>(Resource.Id.buildingRecyclerView);
            addButton = FindViewById<FloatingActionButton>(Resource.Id.addnewBuilding_button);

            recyclerView.HasFixedSize = true;
            recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(this, ViewModel));

            refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.buildingRefresher);
            refresher.SetColorSchemeColors(Resource.Color.accent);
            addButton.Click += AddButton_Click;
            addButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            
            Toolbar.MenuItemClick += (sender, e) =>
            {
                var intent = new Intent(this, typeof(LoginActivity));
                ap.SaveUserId("0");
                StartActivity(intent);
            };
           
            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                SupportActionBar.Title = facility.Name;
            }

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var newIntent = new Intent(this, typeof(MainActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            StartActivity(newIntent);
            Finish();
            return true;
        }

        void AddButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AddBuildingActivity)); ;
            StartActivity(intent);
        }
        protected override void OnRestart()
        {
            base.OnRestart();
            Recreate();
        }

        protected async override void OnStart()
        {
            base.OnStart();
            if (ViewModel.Buildings.Count == 0)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                await ViewModel.ExecuteBuildingsCommand(facilityId);
                recyclerView.HasFixedSize = true;
                recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(this, ViewModel));
                messageDialog.HideLoading();
            }

            refresher.Refresh += Refresher_Refresh;
            adapter.ItemClick += Adapter_ItemClick;
        }
        
        protected override void OnStop()
        {
            base.OnStop();
            refresher.Refresh -= Refresher_Refresh;
            adapter.ItemClick -= Adapter_ItemClick;
        }

        void Adapter_ItemClick(object sender, RecyclerClickEventArgs e)
        {
            var item = ViewModel.Buildings[e.Position];
            var intent = new Intent(this, typeof(AddBuildingActivity));
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            this.StartActivity(intent);
        }

        async void Refresher_Refresh(object sender, EventArgs e)
        {
            await ViewModel.ExecuteBuildingsCommand(facilityId);
            recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(this, ViewModel));
            refresher.Refreshing = false;
            refresher.Refresh += Refresher_Refresh;
            adapter.ItemClick += Adapter_ItemClick;
        }
    }

    class BrowseBuildingsAdapter : BaseRecycleViewAdapter
    {
        BuildingsViewModel viewModel;
        Activity activity;

        public BrowseBuildingsAdapter(Activity activity, BuildingsViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.activity = activity;

            this.viewModel.Buildings.CollectionChanged += (sender, args) =>
            {
                this.activity.RunOnUiThread(NotifyDataSetChanged);
            };
        }


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Setup your layout here
            View itemView = null;
            var id = Resource.Layout.building_card;
            itemView = LayoutInflater.From(parent.Context).Inflate(id, parent, false);

            var vh = new MyViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = viewModel.Buildings[position];
            // Replace the contents of the view with that element
            var myHolder = holder as MyViewHolder;
            myHolder.TextView.Text = item.BuildingName;
            myHolder.DetailTextView.Text = item.BuildingNumber;

            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
            Bitmap bit = ap.SetImageBitmap(ap.CreateDirectoryForPictures() + "/" + item.Photo);
            if (bit != null)
                myHolder.ImageView.SetImageBitmap(bit);
        }

        public override int ItemCount => viewModel.Buildings.Count;
    }

    public class MyViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextView { get; set; }
        public TextView DetailTextView { get; set; }
        public ImageView ImageView { get; set; }

        public MyViewHolder(View itemView, Action<RecyclerClickEventArgs> clickListener,
                            Action<RecyclerClickEventArgs> longClickListener) : base(itemView)
        {
            TextView = itemView.FindViewById<TextView>(Android.Resource.Id.Text1);
            DetailTextView = itemView.FindViewById<TextView>(Android.Resource.Id.Text2);
            DetailTextView = itemView.FindViewById<TextView>(Android.Resource.Id.Text2);
            ImageView = itemView.FindViewById<ImageView>(Resource.Id.buildings_photo);

            itemView.Click += (sender, e) => clickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }
}