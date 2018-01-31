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

namespace TheDataProject.Droid.Helpers
{
    public class ImageManager
    {
        private Context mContext;
        public ImageManager(Context context)
        {
            this.mContext = context;
        }
        public Bitmap ConvertStringToBitMap(String encodedString)
        {
            try
            {
                byte[] encodeByte = Base64.Decode(encodedString, Base64.Default);
                Bitmap bitmap = BitmapFactory.DecodeByteArray(encodeByte, 0, encodeByte.Length);

                int targetWidth = 60;
                int targetHeight = 60;
                Bitmap targetBitmap = Bitmap.CreateBitmap(targetWidth,
                                    targetHeight, Bitmap.Config.Argb8888);

                Canvas canvas = new Canvas(targetBitmap);
                Path path = new Path();
                path.AddCircle(((float)targetWidth - 1) / 2,
                    ((float)targetHeight - 1) / 2,
                    (Math.Min(((float)targetWidth),
                    ((float)targetHeight)) / 2),
                    Path.Direction.Ccw);

                canvas.ClipPath(path);
                Bitmap sourceBitmap = bitmap;
                canvas.DrawBitmap(sourceBitmap,
                    new Rect(0, 0, sourceBitmap.Width,
                    sourceBitmap.Height),
                    new Rect(0, 0, targetWidth, targetHeight), null);
                return targetBitmap;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}