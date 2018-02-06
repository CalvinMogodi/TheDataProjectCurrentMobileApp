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

namespace TheDataProject.Droid
{
    public class FacilityFragment : Android.Support.V4.App.Fragment, IFragmentVisible
    {
        public static FacilityFragment NewInstance() =>
            new FacilityFragment { Arguments = new Bundle() };

        BrowseFacilitiesAdapter adapter;
        SwipeRefreshLayout refresher;
        RecyclerView recyclerView;

        ProgressBar progress;
        public static FacilitiesViewModel ViewModel { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();

            View view = inflater.Inflate(Resource.Layout.fragment_facility, container, false);
            recyclerView =
                view.FindViewById<RecyclerView>(Resource.Id.facilityRecyclerView);

            recyclerView.HasFixedSize = true;
            recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel));

            refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.SetColorSchemeColors(Resource.Color.accent);

            progress = view.FindViewById<ProgressBar>(Resource.Id.progressbar_loading);
            progress.Visibility = ViewStates.Gone;

            return view;
        }

        public async override void OnStart()
        {
            base.OnStart();
            if (ViewModel.Facilities.Count == 0) {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                await ViewModel.ExecuteFacilitiesCommand(1);
                recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel));
                             
                if (ViewModel.Facilities.Count == 0)
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
            var item = ViewModel.Facilities[e.Position];
            var intent = new Intent(Activity, typeof(FacilityDetailActivity));

            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(item));
            Activity.StartActivity(intent);
        }

        async void Refresher_Refresh(object sender, EventArgs e)
        {
            await ViewModel.ExecuteFacilitiesCommand(1);
            recyclerView.SetAdapter(adapter = new BrowseFacilitiesAdapter(Activity, ViewModel));
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

        public BrowseFacilitiesAdapter(Activity activity, FacilitiesViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.activity = activity;

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
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = viewModel.Facilities[position];

            // Replace the contents of the view with that element
            var myHolder = holder as MyViewHolder;
            myHolder.TextView.Text = item.Name;
            myHolder.DetailTextView.Text = item.ClientCode;
            Bitmap bitmap = ((BitmapDrawable)myHolder.ImageView.Drawable).Bitmap;
            if (bitmap != null)
            {
                myHolder.ImageView.SetImageBitmap(bitmap);
            }
        }

        public override int ItemCount => viewModel.Facilities.Count;
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
            ImageView = itemView.FindViewById<ImageView>(Resource.Id.facility_photo);
            itemView.Click += (sender, e) => clickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new RecyclerClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }
}
