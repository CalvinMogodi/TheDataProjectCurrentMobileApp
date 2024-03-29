﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Graphics;
using Android.Util;
using Java.Net;
using Java.IO;
using Android.Net;
using System.IO;

namespace TheDataProject.Droid.Helpers
{
    public class AppPreferences
    {
        private ISharedPreferences nameSharedPrefs;
        private ISharedPreferencesEditor namePrefsEditor; //Declare Context,Prefrences name and Editor name  
        private Context mContext;
        private static String PREFERENCE_ACCESS_KEY = "PREFERENCE_ACCESS_KEY"; //Value Access Key Name  
        public static String NAME = "NAME"; //Value Variable Name  
        private static readonly string PREFERENCE_ACCESS_List; //Value Access Key Name  
        public static String NAMES; //Value Variable Name  
        public AppPreferences(Context context)
        {
            this.mContext = context;
            nameSharedPrefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
            namePrefsEditor = nameSharedPrefs.Edit();
        }
        public void SaveUserId(string key) // Save data Values  
        {
            namePrefsEditor.PutString(PREFERENCE_ACCESS_KEY, key);
            namePrefsEditor.Commit();
        }
        public void SaveFacilityId(string key) // Save data Values  
        {
            namePrefsEditor.PutString(PREFERENCE_ACCESS_List, key);
            namePrefsEditor.Commit();
        }
        public string GetUserId() // Return Get the Value  
        {
            return nameSharedPrefs.GetString(PREFERENCE_ACCESS_KEY, "");
        }
        public string GetFacilityId() // Return Get the Value  
        {
            return nameSharedPrefs.GetString(PREFERENCE_ACCESS_List, "");
        }

        public Bitmap StringToBitMap(String image)
        {
            try
            {
                byte[] encodeByte = Base64.Decode(image, Base64Flags.Default);
                Bitmap bitmap = BitmapFactory.DecodeByteArray(encodeByte, 0, encodeByte.Length);
                return bitmap;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Bitmap ByteToBitMap(byte[] encodeByte)
        {
            try
            {
                return BitmapFactory.DecodeByteArray(encodeByte, 0, encodeByte.Length);               
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Bitmap SetImageBitmap(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    var image = new Java.IO.File(filePath);
                    return BitmapFactory.DecodeFile(image.AbsolutePath);
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public int GetImageWidth(int width)
        {
            width = width / 2;
            if (width > 500)
            {
                width = width / 3;
            }
            return width;
        }

        public int GetImageHeight(int height)
        {
            height = height / 2;
            if (height > 600)
            {
                height = height / 3;
            }
            return height;
        }

        public Java.IO.File CreateDirectoryForPictures()
        {
            Java.IO.File _dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "TheDataProjectImages");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }

            return _dir;
        }

        public void SaveImage(Bitmap bitmap, string fileName)
        {
            try
            {
                fileName = String.Format(fileName);
                using (var os = new FileStream(CreateDirectoryForPictures() + "/" + fileName, FileMode.CreateNew))
                {
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 95, os);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public string SaveImage(Bitmap bitmap)
        {
            string _fileName;
            try
            {
                _fileName = String.Format("facility_{0}", Guid.NewGuid());
                using (var os = new FileStream(CreateDirectoryForPictures() + "/" + _fileName, FileMode.CreateNew))
                {
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 95, os);
                }
                return _fileName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public bool IsOnline(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            // test for connection
            if (cm.ActiveNetworkInfo != null
                    && cm.ActiveNetworkInfo.IsAvailable
                    && cm.ActiveNetworkInfo.IsConnected)
            {
                return true;
            }
            else
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendMessage("Please make sure you are connected","No Connection");
                return false;
            }
        }

    }
}