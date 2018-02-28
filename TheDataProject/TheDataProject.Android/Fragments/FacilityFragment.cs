using System;
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
        public static FacilityFragment NewInstance() =>
            new FacilityFragment { Arguments = new Bundle() };

        BrowseFacilitiesAdapter adapter;
        SwipeRefreshLayout refresher;
        RecyclerView recyclerView;
        EditText searchedTxt;
        int userId;
        ProgressBar progress;
        AppPreferences ap;
        public SqlLiteManager SqlLiteManager { get; set; }
        public static FacilitiesViewModel ViewModel { get; set; }

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
            searchedTxt = view.FindViewById<EditText>(Resource.Id.searchedTxt);

            searchedTxt.TextChanged += Search_Facilities;

            recyclerView.HasFixedSize = true;
            recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel, userId));

            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.SetColorSchemeColors(Resource.Color.accent);

            progress = view.FindViewById<ProgressBar>(Resource.Id.progressbar_loading);
            progress.Visibility = ViewStates.Gone;

            return view;
        }

        void Search_Facilities(object sender, EventArgs e)
        {
            if (searchedTxt.Text.Trim().Length > 1)
            {
                var newList = ViewModel.OriginalFacilities.Where(f => f.Name.Contains(searchedTxt.Text.Trim()) || f.ClientCode.Contains(searchedTxt.Text.Trim()));
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
            // var intent = new Intent(Activity, typeof(FacilityDetailActivity));
            var intent = new Intent(Activity, typeof(FacilityInformationActivity));
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

       
    }

    class BrowseFacilitiesAdapter : BaseRecycleViewAdapter
    {
        FacilitiesViewModel viewModel;
        Activity activity;
        int userId = 0;

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
            myHolder.Button.Click += (sender, e) => {
                Submit_Click(item);
            };
            myHolder.Location.LongClick += (sender, e) => {
                Open_Map(item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
            };
            if (item.Location != null)
            {
                myHolder.StreetAddress.Text = String.Format("Address: {0}",item.Location.StreetAddress);
                myHolder.Suburb.Text = String.Format("               {0}",item.Location.Suburb);
                if (item.Location.GPSCoordinates != null)
                {
                    myHolder.Location.Text = String.Format("Latitude: {0} Longitude: {1}", item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
                }
            }

            myHolder.Location.Text = String.Format("Latitude: {0} Longitude: {1}", 0,0);
            if (item.Location != null)
            {
                if (item.Location.GPSCoordinates != null) {
                    myHolder.Location.Text = String.Format("Latitude: {0} Longitude: {1}", item.Location.GPSCoordinates.Latitude, item.Location.GPSCoordinates.Longitude);
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
        async void Submit_Click(Facility facility)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();
            BuildingsViewModel ViewModel = new BuildingsViewModel();
            await ViewModel.ExecuteBuildingsCommand(facility.Id);
            var buildings = ViewModel.Buildings;           

            if (!ValidateForm(facility, buildings, messageDialog))
            {
                messageDialog.HideLoading();
                return;
            }               

            facility.Status = "Submitted";
            facility.ModifiedUserId = userId;
            facility.ModifiedDate = new DateTime();
            bool isUpdated = await viewModel.ExecuteUpdateFacilityCommand(facility);
            messageDialog.HideLoading();
            if (isUpdated)
            {
                viewModel.Facilities.Remove(viewModel.Facilities.Where(s => s.Id == facility.Id).Single());
                messageDialog.SendToast("Facility is submitted for approval.");
                var myActivity = (MainActivity)this.activity;
                myActivity.Recreate();
            }
            else {
                messageDialog.SendToast("Unable to submitted facility for approval.");
            }
        }
        public override int ItemCount => viewModel.Facilities.Count;

        private bool ValidateForm(Facility facility, ObservableCollection<Building> buildings, MessageDialog messageDialog)
        {
            Validations validation = new Validations();           

            bool isValid = true;
            bool deedsInfoIsRequired = false;
            bool locationfoIsRequired = false;
            bool pictureIsRequired = false;
            bool buildingPictureIsRequired = false;
            bool buildingLocationIsRequired = false;

            foreach (var building in buildings)
            {
                if (String.IsNullOrEmpty(building.Photo))
                {
                    buildingPictureIsRequired = true;
                    isValid = false;
                }
                if (building.GPSCoordinates == null)
                {
                    buildingLocationIsRequired = true;
                    isValid = false;
                }
            }

            if (facility.DeedsInfo == null)
            {
                deedsInfoIsRequired = true;
                isValid = false;
            }
            if (facility.Location == null)
            {
                locationfoIsRequired = true;
                 isValid = false;
            }
            
            if (!validation.IsRequired(facility.IDPicture))
            {
                pictureIsRequired = true;
                isValid = false;
            }

            if (deedsInfoIsRequired || locationfoIsRequired || pictureIsRequired || buildingLocationIsRequired || buildingPictureIsRequired) 
            {
                if (deedsInfoIsRequired && locationfoIsRequired && pictureIsRequired)
                    messageDialog.SendToast("Please add an image, location information and deeds information");
                else if (deedsInfoIsRequired)
                    messageDialog.SendToast("Please capture deeds information.");
                else if(locationfoIsRequired)
                    messageDialog.SendToast("Please capture location information.");
                else if(pictureIsRequired)
                    messageDialog.SendToast("Please add an image.");
                else if (buildingPictureIsRequired && buildingLocationIsRequired)
                    messageDialog.SendToast("Please add an image and location for all the buildings.");
                else if (buildingPictureIsRequired)
                    messageDialog.SendToast("Please add an image for all the buildings.");
                else if (buildingLocationIsRequired)
                    messageDialog.SendToast("Please add location for all the buildings.");
            }
            return isValid;
        }
    }

    public class MyViewHolder : RecyclerView.ViewHolder
    {
        public TextView ClientCode { get; set; }
        public TextView FacilityName { get; set; }
        public TextView StreetAddress { get; set; }
        public TextView Suburb { get; set; }
        public TextView Location { get; set; }
        public ImageView ImageView { get; set; }
        public Button Button { get; set; }

        public MyViewHolder(View itemView, Action<RecyclerClickEventArgs> clickListener,
                            Action<RecyclerClickEventArgs> longClickListener) : base(itemView)
        {
            FacilityName = itemView.FindViewById<TextView>(Resource.Id.f_text1);
            ClientCode = itemView.FindViewById<TextView>(Resource.Id.f_text2);
            StreetAddress = itemView.FindViewById<TextView>(Resource.Id.f_text3);
            Suburb = itemView.FindViewById<TextView>(Resource.Id.f_text5);
            Location = itemView.FindViewById<TextView>(Resource.Id.f_text4);
            ImageView = itemView.FindViewById<ImageView>(Resource.Id.facility_photo);
            Button = itemView.FindViewById<Button>(Resource.Id.submitfacilitybtn);
            itemView.Click += (sender, e) => clickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }
}
