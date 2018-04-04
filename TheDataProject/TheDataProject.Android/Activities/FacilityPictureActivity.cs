using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.ViewModels;
using TheDataProject.Models;
using Android.Views;
using Android.Content.PM;
using TheDataProject.Droid.Helpers;
using Android.Content;
using System.Collections.Generic;
using Android.Graphics;
using System.IO;
using Android.Graphics.Drawables;
using Android.Util;
using System.Linq;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Facility Picture Activity", AlwaysRetainTaskState = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class FacilityPictureActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button takeaphotoButton, selectPictureButton, selectSecondImgButton, cancelPitureButton, takeaSecondImgButton, donePitureButton;
        ImageView facilityPhoto , secondFacilityPhoto;
        List<string> imageNames;
        public Facility facility;
        public static FacilitiesViewModel ViewModel { get; set; }
        public static Java.IO.File _dir;
        public static readonly int TakeImageId = 1000;
        public static readonly int SelectImageId = 2000;
        public bool IsFirstPhoto = false;
        public bool FirstPhotoIsChanged = false;
        public bool SecondPhotoIsChanged = false;
        public Java.IO.File _File;
        AppPreferences ap;
        public Android.Support.V7.Widget.Toolbar Toolbar
        {
            get;
            set;
        }

        //protected override int LayoutResource => Resource.Layout.activity_facility_picture;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_facility_picture);
            cancelPitureButton = FindViewById<Button>(Resource.Id.pcancel_button);
            donePitureButton = FindViewById<Button>(Resource.Id.pdone_button);
            secondFacilityPhoto = FindViewById<ImageView>(Resource.Id.facility_secondphoto_imageview);
            facilityPhoto = FindViewById<ImageView>(Resource.Id.facility_photo_imageview);
            ap = new AppPreferences(Android.App.Application.Context);

            takeaphotoButton = FindViewById<Button>(Resource.Id.img_takeaphoto);
            selectPictureButton = FindViewById<Button>(Resource.Id.img_selectpicture);
            selectSecondImgButton = FindViewById<Button>(Resource.Id.secondimg_selectpicture);
            takeaSecondImgButton = FindViewById<Button>(Resource.Id.secondimg_takeaphoto);

            selectPictureButton.Click += SelectAPicture;
            takeaphotoButton.Click += TakeAPicture;
            selectSecondImgButton.Click += SelectASecondPicture;
            takeaSecondImgButton.Click += TakeASecondPicture;

            cancelPitureButton.Click += CancelButton_Click;
            donePitureButton.Click += SaveButton_Click;
            _dir = CreateDirectoryForPictures();
            Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (Toolbar != null)
            {
                SetSupportActionBar(Toolbar);
                SupportActionBar.Title = "Facility Pictures";
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
            }

            var data = Intent.GetStringExtra("data");
            facility = new Facility();
            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                imageNames = facility.IDPicture == null ? new List<string>() : facility.IDPicture.Split(',').ToList();
                if (imageNames.Count > 0)
                    GetImages(ap);
            }
        }
        private async void GetImages(AppPreferences ap)
        {
            if (!String.IsNullOrEmpty(imageNames[0]))
            {
                Bitmap bit = ap.SetImageBitmap(_dir + "/" + imageNames[0]);
                if (bit != null)
                    facilityPhoto.SetImageBitmap(bit);
                else if (bit == null && !String.IsNullOrEmpty(imageNames[0]))
                {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Models.Picture picture = await pictureViewModel.ExecuteGetPictureCommand(imageNames[0]);
                    if (picture != null)
                    {
                        var _bit = ap.StringToBitMap(picture.File);
                        if (_bit != null)
                            ap.SaveImage(_bit, imageNames[0]);
                        facilityPhoto.SetImageBitmap(_bit);
                    }
                }
            }
            if (imageNames.Count > 1)
            {
                Bitmap bit = ap.SetImageBitmap(_dir + "/" + imageNames[1]);
                if (bit != null)
                    secondFacilityPhoto.SetImageBitmap(bit);
                else if (bit == null && !String.IsNullOrEmpty(imageNames[1]))
                {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Models.Picture picture = await pictureViewModel.ExecuteGetPictureCommand(imageNames[1]);
                    if (picture != null)
                    {
                        var _bit = ap.StringToBitMap(picture.File);
                        if (_bit != null)
                            ap.SaveImage(_bit, imageNames[1]);
                        secondFacilityPhoto.SetImageBitmap(_bit);
                    }
                }
            }
        }


        void CancelButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(FacilityDetailActivity));
            Context mContext = Android.App.Application.Context;
            AppPreferences ap = new AppPreferences(mContext);
            ap.SaveFacilityId(facility.Id.ToString());
            facility.Buildings = new List<Building>();
            intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
            this.StartActivity(intent);
            Finish();
        }
        async void SaveButton_Click(object sender, EventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();
            if (imageNames.Count() == 0)
            {
                imageNames = new List<string>();
            }
            
            if (FirstPhotoIsChanged)
            {                
                string thisFileName = ap.SaveImage(((BitmapDrawable)facilityPhoto.Drawable).Bitmap);
                if (imageNames.Count() > 0)
                    imageNames[0] = thisFileName;
                else
                    imageNames.Add(thisFileName);

            }
            if (SecondPhotoIsChanged)
            {
                var _fileName = String.Format("facility_{0}", Guid.NewGuid());
                ap.SaveImage(((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap, _fileName);
                if (imageNames.Count() > 1)
                    imageNames[1] = _fileName;
                else
                    imageNames.Add(_fileName);
            }
            if (FirstPhotoIsChanged)
            {
                facility.IDPicture = "";

                foreach (var name in imageNames)
                {
                    if (!String.IsNullOrEmpty(name))
                    {
                        if (String.IsNullOrEmpty(facility.IDPicture))
                            facility.IDPicture = name;
                        else
                            facility.IDPicture = facility.IDPicture + "," + name;
                    }
                }
            }


            bool isUpdated = await ViewModel.ExecuteUpdateFacilityCommand(facility);
            if (isUpdated)
            {
                PictureViewModel pictureViewModel = new PictureViewModel();
                List<Models.Picture> pictures = new List<Models.Picture>();
                if (FirstPhotoIsChanged)
                {
                    Bitmap _bm = ((BitmapDrawable)facilityPhoto.Drawable).Bitmap;
                    string file = "";
                    if (_bm != null)
                    {
                        MemoryStream stream = new MemoryStream();
                        _bm.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        byte[] ba = stream.ToArray();
                        file = Base64.EncodeToString(ba, Base64.Default);
                    }

                    Models.Picture picture = new Models.Picture()
                    {
                        Name = imageNames[0],
                        File = file,
                    };
                    pictures.Add(picture);
                }
                bool isSuccess = await pictureViewModel.ExecuteSavePictureCommand(pictures);

                messageDialog.HideLoading();
                messageDialog.SendToast("Pictures are saved successful.");

                messageDialog.HideLoading();
                if (isSuccess)
                {
                    messageDialog.SendToast("Deeds information is saved successful.");
                    Finish();
                }
                else
                {
                    messageDialog.SendToast("Error occurred: Unable to save deed information.");
                }
               
            }
            else
            {
                messageDialog.HideLoading();
                messageDialog.SendToast("Pictures are not saved successful.");
            }
        }

        async void TakeAPicture(object sender, EventArgs e)
        {
            IsFirstPhoto = true;
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            _File = new Java.IO.File(CreateDirectoryForPictures(), String.Format("{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_File));
            StartActivityForResult(intent, TakeImageId);
        }

        async void TakeASecondPicture(object sender, EventArgs e)
        {
            IsFirstPhoto = false;
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            _File = new Java.IO.File(CreateDirectoryForPictures(), String.Format("{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_File));
            StartActivityForResult(intent, TakeImageId);
        }


        async void SelectASecondPicture(object sender, EventArgs e)
        {
            IsFirstPhoto = false;
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(imageIntent, "Select Photo"), SelectImageId);
        }
        async void SelectAPicture(object sender, EventArgs e)
        {
            IsFirstPhoto = true;
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(imageIntent, "Select Photo"), SelectImageId);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AppPreferences ap = new AppPreferences(Application.Context);

            if (requestCode == TakeImageId && resultCode != 0 && _File != null)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_File);
                mediaScanIntent.SetData(contentUri);
                Application.Context.SendBroadcast(mediaScanIntent);
                int height = Resources.DisplayMetrics.HeightPixels;
                int width = Resources.DisplayMetrics.WidthPixels;
                Android.Graphics.Bitmap bitmap = _File.Path.LoadAndResizeBitmap(ap.GetImageWidth(width), ap.GetImageHeight(height));
                if (bitmap != null)
                {
                   if (IsFirstPhoto) {
                        facilityPhoto.SetImageBitmap(bitmap);
                        FirstPhotoIsChanged = true;
                    }                        
                   else{
                        secondFacilityPhoto.SetImageBitmap(bitmap);
                        SecondPhotoIsChanged = true;
                    }   
                }
                bitmap = null;
                GC.Collect();
            }
            else
            {
                if (data != null)
                {
                    Android.Net.Uri uri = data.Data;
                    if (IsFirstPhoto)
                    {
                        facilityPhoto.SetImageURI(uri);
                        FirstPhotoIsChanged = true;
                    }
                    else
                    {
                        secondFacilityPhoto.SetImageURI(uri);
                        SecondPhotoIsChanged = true;
                    }
                    GC.Collect();
                }
            }
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
    }
}