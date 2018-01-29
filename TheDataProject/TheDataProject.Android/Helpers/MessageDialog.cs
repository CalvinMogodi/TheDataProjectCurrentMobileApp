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
using Plugin.CurrentActivity;

namespace TheDataProject.Droid.Helpers
{
    public class MessageDialog
    {
        ProgressDialog progress;
        public void ShowLoading()
        {
            var activity = CrossCurrentActivity.Current.Activity;
            progress = new ProgressDialog(activity, Resource.Style.MyTheme)
            {
                Indeterminate = true
            };
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage("Loading...");
            progress.SetCancelable(false);
            //progress.Create();
            progress.Show();
        }
        public void HideLoading()
        {
            progress.Cancel();
        }

       
        public void SendMessage(string message, string title = null)
        {
            var activity = CrossCurrentActivity.Current.Activity;
            var builder = new AlertDialog.Builder(activity);
            builder
                .SetTitle(title ?? string.Empty)
                .SetMessage(message)
                .SetPositiveButton(Android.Resource.String.Ok, delegate
                {

                });

            activity.RunOnUiThread(() =>
            {
                AlertDialog alert = builder.Create();
                alert.Show();
            });
        }


        public void SendToast(string message)
        {
            var activity = CrossCurrentActivity.Current.Activity;
            activity.RunOnUiThread(() =>
            {
                Toast.MakeText(activity, message, ToastLength.Long).Show();
            });

        }


        public void SendConfirmation(string message, string title, Action<bool> confirmationAction)
        {
            var activity = CrossCurrentActivity.Current.Activity;
            var builder = new AlertDialog.Builder(activity);
            builder
            .SetTitle(title ?? string.Empty)
            .SetMessage(message)
            .SetPositiveButton(Android.Resource.String.Ok, delegate
            {
                confirmationAction(true);
            }).SetNegativeButton(Android.Resource.String.Cancel, delegate
            {
                confirmationAction(false);
            });

            activity.RunOnUiThread(() =>
            {
                AlertDialog alert = builder.Create();
                alert.Show();
            });
        }
    }
}