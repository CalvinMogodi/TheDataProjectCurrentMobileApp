using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using TheDataProject.ViewModels;
using Android.Support.Design.Widget;
using TheDataProject.Droid.Activities;
using TheDataProject.Droid.Helpers;

namespace TheDataProject.Droid.Fragments
{
    public class FacilityBuildingFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {
        public static FacilityBuildingFragment NewInstance() =>
            new FacilityBuildingFragment { Arguments = new Bundle() };

        BrowseBuildingsAdapter adapter;
        SwipeRefreshLayout refresher;
        FloatingActionButton addButton;
        RecyclerView recyclerView;
        ProgressBar progress;
        int facilityId;
        public static BuildingsViewModel ViewModel { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new BuildingsViewModel();

            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            facilityId = Convert.ToInt32(ap.GetFacilityId());

            View view = inflater.Inflate(Resource.Layout.fragment_facility_building, container, false);

            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.buildingRecyclerView);
            addButton = view.FindViewById<FloatingActionButton>(Resource.Id.addnewBuilding_button);

            recyclerView.HasFixedSize = true;
            recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(Activity, ViewModel));

            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.buildingRefresher);
            refresher.SetColorSchemeColors(Resource.Color.accent);

            progress = view.FindViewById<ProgressBar>(Resource.Id.buildingprogressbar_loading);
            progress.Visibility = ViewStates.Gone;
            addButton.Click += AddButton_Click;
            addButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            return view;
        }

        void AddButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(AddBuildingActivity)); ;
            StartActivity(intent);
        }


        public async override void OnStart()
        {
            base.OnStart();
            if (ViewModel.Buildings.Count == 0)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                await ViewModel.ExecuteBuildingsCommand(facilityId);
                recyclerView.HasFixedSize = true;
                recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(Activity, ViewModel));
                if (ViewModel.Buildings.Count == 0)
                {
                    messageDialog.SendMessage("There are no facilities that are assinged to this profile.", "No Facilities Found");
                }
                messageDialog.HideLoading();
            }

            refresher.Refresh += Refresher_Refresh;
            adapter.ItemClick += Adapter_ItemClick;
        }

        public override void OnStop()
        {
            base.OnStop();
            refresher.Refresh -= Refresher_Refresh;
            adapter.ItemClick -= Adapter_ItemClick;
        }

        void Adapter_ItemClick(object sender, RecyclerClickEventArgs e)
        {
            var item = ViewModel.Buildings[e.Position];
            var intent = new Intent(Activity, typeof(AddBuildingActivity));
            item.Photo = "";            
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            Activity.StartActivity(intent);
        }

        async void Refresher_Refresh(object sender, EventArgs e)
        {
            await ViewModel.ExecuteBuildingsCommand(facilityId);
            recyclerView.SetAdapter(adapter = new BrowseBuildingsAdapter(Activity, ViewModel));
            refresher.Refreshing = false;
            refresher.Refresh += Refresher_Refresh;
            adapter.ItemClick += Adapter_ItemClick;
        }

        public void BecameVisible()
        {

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
        }

        public override int ItemCount => viewModel.Buildings.Count;
    }

    public class MyViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextView { get; set; }
        public TextView DetailTextView { get; set; }

        public MyViewHolder(View itemView, Action<RecyclerClickEventArgs> clickListener,
                            Action<RecyclerClickEventArgs> longClickListener) : base(itemView)
        {
            TextView = itemView.FindViewById<TextView>(Android.Resource.Id.Text1);
            DetailTextView = itemView.FindViewById<TextView>(Android.Resource.Id.Text2);
            itemView.Click += (sender, e) => clickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }
}
