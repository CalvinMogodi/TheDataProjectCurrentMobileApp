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
using Android.Graphics;
using Android.Util;
using Android.Provider;
using Android.Content.PM;

namespace TheDataProject.Droid.Helpers
{
    public static class ImageManager
    {

        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
            {
                // First we get the the dimensions of the file on disk
                BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(fileName, options);

                // Next we calculate the ratio that we need to resize the image by
                // to fit the requested dimensions.
                int outHeight = options.OutHeight;
                int outWidth = options.OutWidth;
                int inSampleSize = 1;

                if (outHeight > height || outWidth > width)
                {
                    inSampleSize = outWidth > outHeight
                                       ? outHeight / height
                                       : outWidth / width;
                }

                // Now we will load the image and have BitmapFactory resize it for us.
                options.InSampleSize = inSampleSize;
                options.InJustDecodeBounds = false;
                Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

                return resizedBitmap;
          
        }
        
    }
}