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
using Android.Content.PM;
using TheDataProject.Models;
using TheDataProject.ViewModels;
using TheDataProject.Droid.Helpers;
using System.Drawing;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Login", Icon = "@mipmap/icon",
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : Activity
    {
       
        EditText username, password;
        TextView message;
        Button signInBtn, signInOffLineBtn;

        
        public bool FormIsValid { get; set; }
        public User User { get; set; }
        public LoginViewModel ViewModel { get; set; }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            // Create your application here
            Initialize();
        }

        private void Initialize()
        {

            // Create your application here
            username = FindViewById<EditText>(Resource.Id.etlogin_username);
            password = FindViewById<EditText>(Resource.Id.etlogin_password);            
            message = FindViewById<TextView>(Resource.Id.tvlogin_message);
            signInBtn = FindViewById<Button>(Resource.Id.btnlogin_signin);
            signInOffLineBtn = FindViewById<Button>(Resource.Id.btnlogin_signinoffline);
            
            signInBtn.SetBackgroundColor(Android.Graphics.Color.ParseColor("#101e45"));
            ViewModel = new LoginViewModel();
            signInBtn.Click += SignIn_Click;
        }

        private async void SignIn_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();

            EncryptionHelper encryptionHelper = new EncryptionHelper();
            message.Text = "";
            var _user = new User()
            {
                Username = username.Text,
                Password = password.Text,
            };
            
            User user = await ViewModel.ExecuteLoginCommand(_user);

            if (user.RespondMessage != null)
                message.Text = user.RespondMessage;
            else
            {
                var newIntent = new Intent(this, typeof(MainActivity));
                newIntent.AddFlags(ActivityFlags.ClearTop);
                newIntent.AddFlags(ActivityFlags.SingleTop);
                Context mContext = Android.App.Application.Context;
                AppPreferences ap = new AppPreferences(mContext);
                ap.SaveUserId(user.Id.ToString());

                StartActivity(newIntent);
            
                Finish();
            }

            messageDialog.HideLoading();
        }

        private async void Cancel_Click(object sender, EventArgs e)
        {
            username.Text = "";
            password.Text = "";
            message.Text = "";
        }

        private bool ValidateForm()
        {
            Validations validation = new Validations();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            FormIsValid = true;            

            if (!validation.IsValidEmail(username.Text))
            {
                username.SetError("Invalid username", icon);
                FormIsValid = false;
            }
            if (!validation.IsRequired(username.Text))
            {
                username.SetError("This field is required", icon);
                FormIsValid = false;
            }

            if (!validation.IsRequired(password.Text))
            {
                password.SetError("This field is required", icon);
                FormIsValid = false;
            }

            return FormIsValid;
        }

    }
}