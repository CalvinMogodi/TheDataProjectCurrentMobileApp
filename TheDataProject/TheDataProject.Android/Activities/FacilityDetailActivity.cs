using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Widget;
using TheDataProject.Droid.Fragments;

namespace TheDataProject.Droid
{
    [Activity(Label = "Details", ParentActivity = typeof(MainActivity), LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.support.PARENT_ACTIVITY", Value = ".MainActivity")]
    public class FacilityDetailActivity : BaseActivity
    {
        /// <summary>
        /// Specify the layout to inflace
        /// </summary>
        protected override int LayoutResource => Resource.Layout.activity_facility;

        ViewPager pager;
        TabsAdapter adapter;

        FacilityDetailViewModel viewModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var data = Intent.GetStringExtra("data");

            var item = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
            viewModel = new FacilityDetailViewModel(item);
            

            //SupportActionBar.Title = item.Name;
            adapter = new TabsAdapter(this, SupportFragmentManager);
            pager = FindViewById<ViewPager>(Resource.Id.facilityviewpager);
            var tabs = FindViewById<TabLayout>(Resource.Id.facilitytabs);
            pager.Adapter = adapter;
            //tabs.SetupWithViewPager(pager);
            pager.OffscreenPageLimit = 3;

            pager.PageSelected += (sender, args) =>
            {
                var fragment = adapter.InstantiateItem(pager, args.Position) as IFragmentVisible;

                fragment?.BecameVisible();
            };
            //SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            //SupportActionBar.SetHomeButtonEnabled(false);

        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        class TabsAdapter : FragmentStatePagerAdapter
        {
            string[] titles;

            public override int Count => titles.Length;

            public TabsAdapter(Context context, Android.Support.V4.App.FragmentManager fm) : base(fm)
            {
                titles = context.Resources.GetTextArray(Resource.Array.facilitySections);
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) =>
                                new Java.Lang.String(titles[position]);

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0: return FacilityInformationFragment.NewInstance();
                    case 1: return FacilityBuildingFragment.NewInstance();
                }
                return null;
            }

            public override int GetItemPosition(Java.Lang.Object frag) => PositionNone;
        }
    }
}
