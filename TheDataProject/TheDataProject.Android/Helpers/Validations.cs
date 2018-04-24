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
using System.Text.RegularExpressions;
using Android.Net;

namespace TheDataProject.Droid.Helpers
{
    public class Validations
    {
        static Regex ValidEmailRegex = CreateValidEmailRegex();

        private static Regex CreateValidEmailRegex()
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern);
        }

        public bool IsValidEmail(string emailAddress)
        {
            bool isValid = ValidEmailRegex.IsMatch(emailAddress);

            return isValid;
        }

        public bool IsValidPassword(string pass)
        {
            if (!string.IsNullOrEmpty(pass) && pass.Length > 6)
                return true;
            return false;
        }

        public bool IsRequired(string text)
        {
            if (!string.IsNullOrEmpty(text))
                return true;
            return false;
        }

        public bool IsValidPhone(string number)
        {
            if (!string.IsNullOrEmpty(number) && number.Length == 10)
                return true;
            return false;
        }

        public bool IsValidOTP(string otp)
        {
            if (!string.IsNullOrEmpty(otp) && otp.Length == 5)
                return true;
            return false;
        }

        public bool IsValidDatetime(string date)
        {
            try
            {
                Convert.ToDateTime(date);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void IsOnline(Context context)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService("");
            bool isOnline = connectivityManager.ActiveNetworkInfo.IsConnected;
            if (!isOnline)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendMessage("No internet connection please check your wifi or data settings", "No Internet Connection");

            }
        }
    }
}