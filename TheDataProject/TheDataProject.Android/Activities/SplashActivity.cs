﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using TheDataProject.Droid.Activities;

namespace TheDataProject.Droid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var newIntent = new Intent(this, typeof(LoginActivity));
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            StartActivity(newIntent);
            Finish();
        }
    }
}
