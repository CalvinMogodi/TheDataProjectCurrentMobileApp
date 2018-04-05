using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using TheDataProject.Droid.Activities;
using TheDataProject.Droid.Helpers;
using System;
using Android.Webkit;
using Android.Views.InputMethods;

namespace TheDataProject.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new string[] { Intent.ActionSearch })]
    [MetaData("android.app.searchable", Resource = "@layout/searchable")]
    public class MainActivity : BaseActivity
    {
        protected override int LayoutResource => Resource.Layout.activity_main;
        TextInputLayout searchBar;
        TextInputEditText searchEditText;
        ViewPager pager;
        TabsAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            string userId = ap.GetUserId();
            if (Convert.ToInt32(userId) == 0)
            {
                var newIntent = new Intent(this, typeof(LoginActivity));
                StartActivity(newIntent);
            }

            adapter = new TabsAdapter(this, SupportFragmentManager);
            pager = FindViewById<ViewPager>(Resource.Id.viewpager);
            searchBar = FindViewById<TextInputLayout>(Resource.Id.search_textInputLayout);
            searchEditText = FindViewById<TextInputEditText>(Resource.Id.searchedTxt);
            var tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            pager.Adapter = adapter;
            tabs.SetupWithViewPager(pager);
            pager.OffscreenPageLimit = 3;

            searchBar.Visibility = ViewStates.Gone;
            pager.PageSelected += (sender, args) =>
            {
                var fragment = adapter.InstantiateItem(pager, args.Position) as IFragmentVisible;

                fragment?.BecameVisible();
            };
            searchEditText.Click += (sender, eventArgs) =>
            {
                searchBar.Visibility = ViewStates.Gone;
                Toolbar.Visibility = ViewStates.Visible;
                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(searchEditText.WindowToken, 0);
            };

            searchEditText.TextChanged += (sender, eventArgs) => {
                if (searchEditText.Text.Trim().Length > 1)
                {
                    FacilityFragment fragment =  (FacilityFragment)SupportFragmentManager.FindFragmentByTag("FacilityFragment");
                    if (fragment != null)
                    {
                        fragment.SearchFacilities(searchEditText.Text.Trim());
                    }   
                }
            };

            Toolbar.MenuItemClick += (sender, e) =>
            {
                var itemTitle = e.Item.TitleFormatted;

                switch (itemTitle.ToString())
                {
                    case "Log Out":
                        var intent = new Intent(this, typeof(LoginActivity));
                        string _userId = "0";
                        ap.SaveUserId(_userId);
                        StartActivity(intent);
                        break;
                    case "Search":
                        searchBar.Visibility = ViewStates.Visible;
                        Toolbar.Visibility = ViewStates.Gone;
                        break;
                }
                        
            };

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            for (int j = 0; j < menu.Size(); j++)
            {
                var item = menu.GetItem(j);
                if (item.ToString() == "Search")
                    item.SetShowAsActionFlags(Android.Views.ShowAsAction.Always);
            }
            return base.OnCreateOptionsMenu(menu);
        }
    }

    class TabsAdapter : FragmentStatePagerAdapter
    {
        string[] titles;

        public override int Count => titles.Length;

        public TabsAdapter(Context context, Android.Support.V4.App.FragmentManager fm) : base(fm)
        {
            titles = context.Resources.GetTextArray(Resource.Array.sections);
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) =>
                            new Java.Lang.String(titles[position]);

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0: return FacilityFragment.NewInstance();
            }
            return null;
        }

        public override int GetItemPosition(Java.Lang.Object frag) => PositionNone;
    }
}
