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
using Android.Support.V4.App;
using TheDataProject.Droid.Fragments;
using Android.Support.V4.View;
using Android.Support.Design.Widget;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "FacilityDetailActivity")]
    public class FacilityDetailActivity : BaseActivity
    {
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

            Bundle mybundle = new Bundle();
            mybundle.PutString("data", Newtonsoft.Json.JsonConvert.SerializeObject(item));

            adapter = new TabsAdapter(this, SupportFragmentManager, mybundle);
            pager = FindViewById<ViewPager>(Resource.Id.viewpager);
            var tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            pager.Adapter = adapter;
            tabs.SetupWithViewPager(pager);
            pager.OffscreenPageLimit = 3;

            Toolbar.MenuItemClick += (sender, e) =>
            {
                var intent = new Intent(this, typeof(LoginActivity)); ;
                StartActivity(intent);
            };

            pager.PageSelected += (sender, args) =>
            {
                var fragment = adapter.InstantiateItem(pager, args.Position) as IFragmentVisible;

                fragment?.BecameVisible();
            };

            SupportActionBar.Title = item.Name;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            Recreate();
        }

        public void OnActivityRestart()
        {
            OnRestart();
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
            public static Bundle newbundle;

            public override int Count => titles.Length;

            public TabsAdapter(Context context, Android.Support.V4.App.FragmentManager fm, Bundle mybundle) : base(fm)
            {
                newbundle = mybundle;
                titles = context.Resources.GetTextArray(Resource.Array.facilitySections);
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) =>
                                new Java.Lang.String(titles[position]);

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0: return FacilityInformationFragment.NewInstance(newbundle);
                    case 1: return FacilityBuildingFragment.NewInstance();
                }
                return null;
            }

            public override int GetItemPosition(Java.Lang.Object frag) => PositionNone;
        }
    }
}