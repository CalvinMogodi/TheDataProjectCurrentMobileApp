﻿using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.App;
using Android.Content;
using TheDataProject.Droid.Helpers;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Text;
using TheDataProject.Models;
using System.Collections.Generic;
using System.Linq;
using TheDataProject.ViewModels;
using System.Collections.ObjectModel;
using TheDataProject.Droid.Activities;

namespace TheDataProject.Droid
{
    public class FacilityFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {
        #region Properties
        public static FacilityFragment NewInstance() => new FacilityFragment { Arguments = new Bundle() };
        BrowseFacilitiesAdapter adapter;
        SwipeRefreshLayout refresher;
        RecyclerView recyclerView;
        EditText searchedTxt;
        int userId;
        ProgressBar progress;
        AppPreferences ap;
        public SqlLiteManager SqlLiteManager { get; set; }
        public static FacilitiesViewModel ViewModel { get; set; }
        #endregion #endregion 

        #region Methods 
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();
            ap = new AppPreferences(Application.Context);
            this.SqlLiteManager = new SqlLiteManager();
            userId = Convert.ToInt32(ap.GetUserId());

            View view = inflater.Inflate(Resource.Layout.fragment_facility, container, false);
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.facilityRecyclerView);
            recyclerView.HasFixedSize = true;
            recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));

            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.SetColorSchemeColors(Resource.Color.accent);

            progress = view.FindViewById<ProgressBar>(Resource.Id.progressbar_loading);
            progress.Visibility = ViewStates.Gone;

            return view;
        }

        public void SearchFacilities(string searchedTxt)
        {
            if (searchedTxt.Length > 1)
            {
                var newList = ViewModel.OriginalFacilities.Where(f => f.Name.ToLower().Contains(searchedTxt.ToLower().Trim()) || f.ClientCode.ToLower().Contains(searchedTxt.ToLower().Trim()));
                ViewModel.Facilities = new ObservableCollection<Facility>();
                foreach (var item in newList)
                {
                    ViewModel.Facilities.Add(item);
                }

                recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));
                refresher.Refreshing = false;
                refresher.Refresh += Refresher_Refresh;
                adapter.ItemClick += Adapter_ItemClick;
            }
            else {
                ViewModel.Facilities = new ObservableCollection<Facility>();
                foreach (var item in ViewModel.OriginalFacilities)
                {
                    ViewModel.Facilities.Add(item);
                }

                recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));
                refresher.Refreshing = false;
                refresher.Refresh += Refresher_Refresh;
                adapter.ItemClick += Adapter_ItemClick;
            }
        }
        public async override void OnStart()
        {
            base.OnStart();
            if (ViewModel.Facilities.Count == 0) {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                if (ap.IsOnline(Application.Context))
                {
                    await ViewModel.ExecuteFacilitiesCommand(userId);
                   // await this.SqlLiteManager.SyncFacilitiesFromAPI(ViewModel.Facilities);
                }
                else {
                    //await this.SqlLiteManager.GetLocalFacilities(userId);
                }
                if (ViewModel.Facilities.Count == 0)
                {
                    messageDialog.SendMessage("There are no facilities that are assinged to this profile.", "No Facilities Found");
                }
                recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));                
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
            var item = ViewModel.Facilities[e.Position];
            var intent = new Intent(Activity, typeof(FacilityDetailActivity));
            //var intent = new Intent(Activity, typeof(FacilityInformationActivity));
            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            ap.SaveFacilityId(item.Id.ToString());
            item.Buildings = new List<Building>();
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            Activity.StartActivity(intent);
        }

        async void Refresher_Refresh(object sender, EventArgs e)
        {
            await ViewModel.ExecuteFacilitiesCommand(userId);
            recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));
            refresher.Refreshing = false;
            refresher.Refresh += Refresher_Refresh;
            adapter.ItemClick += Adapter_ItemClick;
        }

        public void BecameVisible()
        {

        }

        #endregion #endregion 

    }

    class BrowseFacilitiesAdapter : BaseRecycleViewAdapter
    {
        #region Properties
        FacilitiesViewModel viewModel;
        Activity activity;
        int userId = 0;
        #endregion #endregion 

        #region Methods 
        public BrowseFacilitiesAdapter(Activity activity, FacilitiesViewModel viewModel, int _userId)
        {
            this.viewModel = viewModel;
            this.activity = activity;
            userId = _userId;
            View itemViewList;

            this.viewModel.Facilities.CollectionChanged += (sender, args) =>
            {
                this.activity.RunOnUiThread(NotifyDataSetChanged);
            };
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Setup your layout here
            View itemView = null;
            var id = Resource.Layout.facility_card;
            itemView = LayoutInflater.From(parent.Context).Inflate(id, parent, false);
            var vh = new MyViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public async override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = viewModel.Facilities[position];
            Context mContext = Android.App.Application.Context;
            AppPreferences appPreferences = new AppPreferences(mContext);

            // Replace the contents of the view with that element
            var myHolder = holder as MyViewHolder;
            myHolder.FacilityName.Text = item.Name;
            myHolder.ClientCode.Text = item.ClientCode;
           
            myHolder.MyLocation.LongClick += (sender, e) => {
                if (item.Location != null)
                {
                    if (item.Location.GPSCoordinates != null)
                        Open_Map(item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
                }                    
            };
            if (item.Location != null)
            {
                myHolder.StreetAddress.Text = String.Format("Address: {0} {1}",item.Location.StreetAddress, item.Location.Suburb);
                if (item.Location.GPSCoordinates != null)
                {
                    myHolder.Location.Text = String.Format("Lat: {0} Long: {1}", item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
                }
            }

            myHolder.Location.Text = String.Format("Lat: {0} Long: {1}", 0,0);
            if (item.Location != null)
            {
                if (item.Location.GPSCoordinates != null) {
                    myHolder.Location.Text = String.Format("Lat: {0} Long: {1}", item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
                }
            }

            if (item.IDPicture != null)
            {
                List<string> imageNames = item.IDPicture.Split(',').ToList();

                AppPreferences ap = new AppPreferences(Android.App.Application.Context);
                Bitmap bit = ap.SetImageBitmap(ap.CreateDirectoryForPictures() + "/" + imageNames[0]);
                if (bit != null)
                    myHolder.ImageView.SetImageBitmap(bit);
                else
                {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Models.Picture picture = await pictureViewModel.ExecuteGetPictureCommand(imageNames[0]);
                    if (picture != null)
                    {
                        var _bit = ap.StringToBitMap(picture.File);
                        if (_bit != null)
                            myHolder.ImageView.SetImageBitmap(_bit);
                    }
                }
            }
        }

        async void Open_Map(string latitude, string longitude) {           
            
            string labelLocation = "Facility Location";
            String urlAddress = "http://maps.google.com/maps?q=" + latitude + "," + longitude + "(" + labelLocation + ")&iwloc=A&hl=es";
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(urlAddress));
            activity.StartActivity(intent);
        }
       
        public override int ItemCount => viewModel.Facilities.Count;

        #endregion #endregion 
    }

    public class MyViewHolder : RecyclerView.ViewHolder
    {
        #region Properties
        public TextView ClientCode { get; set; }
        public TextView FacilityName { get; set; }
        public TextView StreetAddress { get; set; }
        public ImageView MyLocation { get; set; }
        public TextView Location { get; set; }
        public ImageView ImageView { get; set; }
        #endregion #endregion 

        #region Methods 
        public MyViewHolder(View itemView, Action<RecyclerClickEventArgs> clickListener,
                            Action<RecyclerClickEventArgs> longClickListener) : base(itemView)
        {
            FacilityName = itemView.FindViewById<TextView>(Resource.Id.f_text1);
            ClientCode = itemView.FindViewById<TextView>(Resource.Id.f_text2);
            StreetAddress = itemView.FindViewById<TextView>(Resource.Id.f_text3);
            MyLocation = itemView.FindViewById<ImageView>(Resource.Id.mylocation);
            Location = itemView.FindViewById<TextView>(Resource.Id.f_text4);
            ImageView = itemView.FindViewById<ImageView>(Resource.Id.facility_photo);
            itemView.Click += (sender, e) => clickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
        #endregion #endregion 
    }
}
