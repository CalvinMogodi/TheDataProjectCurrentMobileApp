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
using Android.Graphics.Drawables;
using TheDataProject.Droid.Helpers;
using Android.Provider;
using TheDataProject.ViewModels;
using System.IO;
using Android.Util;
using Android.Support.V7.App;
using TheDataProject.Models;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Facility Picture Activity")]
    public class FacilityPictureActivity : AppCompatActivity
    {
        Button takeaphotoButton, selectPictureButton, cancelButton, doneButton, cancelPitureButton, donePitureButton;
        ImageView facilityPhoto, iImageViewer, secondFacilityPhoto;
        List<string> imageNames;
        public Facility facility;
        public static FacilitiesViewModel ViewModel { get; set; }
        public static Java.IO.File _dir;
        Dialog imageDialog, sameImageDialog;
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
            facilityPhoto = FindViewById<ImageView>(Resource.Id.facility_photo_imageview);
            secondFacilityPhoto = FindViewById<ImageView>(Resource.Id.facility_secondphoto_imageview);
            ap = new AppPreferences(Android.App.Application.Context);
            facilityPhoto.Click += (sender, e) => {
                ShowImage_Click(true);
            };

            secondFacilityPhoto.Click += (sender, e) => {
                ShowImage_Click(false);
            };
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
            if (FirstPhotoIsChanged)
            {
                string thisFileName = ap.SaveImage(((BitmapDrawable)facilityPhoto.Drawable).Bitmap);
                imageNames[0] = thisFileName;
            }
            if (SecondPhotoIsChanged)
            {
                var _fileName = String.Format("facility_{0}", Guid.NewGuid());
                ap.SaveImage(((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap, _fileName);
                imageNames[1] = _fileName;
            }
            if (FirstPhotoIsChanged || SecondPhotoIsChanged)
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
                if (SecondPhotoIsChanged)
                {
                    Bitmap _bm = ((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap;
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
                        Name = imageNames[1],
                        File = file,
                    };

                    pictures.Add(picture);
                }
                await pictureViewModel.ExecuteSavePictureCommand(pictures);
                messageDialog.HideLoading();
                messageDialog.SendToast("Pictures are saved successful.");
                
                var intent = new Intent(this, typeof(FacilityDetailActivity));
                Context mContext = Android.App.Application.Context;
                AppPreferences ap = new AppPreferences(mContext);
                ap.SaveFacilityId(facility.Id.ToString());
                facility.Buildings = new List<Building>();
                intent.PutExtra("data", Newtonsoft.Json.JsonConvert.SerializeObject(facility));
                this.StartActivity(intent);
                Finish();
            }
            else
            {
                messageDialog.HideLoading();
                messageDialog.SendToast("Pictures are not saved successful.");
            }
        }

        public void ShowImage_Click(bool isFirstImage)
        {
            IsFirstPhoto = isFirstImage;
            imageDialog = new Dialog(this);

            imageDialog.SetContentView(Resource.Layout.dialog_select_image);
            takeaphotoButton = imageDialog.FindViewById<Button>(Resource.Id.img_takeaphoto);
            iImageViewer = imageDialog.FindViewById<ImageView>(Resource.Id.imgsi_facilityphoto);
            selectPictureButton = imageDialog.FindViewById<Button>(Resource.Id.img_selectpicture);
            cancelButton = imageDialog.FindViewById<Button>(Resource.Id.sicancel_button);
            doneButton = imageDialog.FindViewById<Button>(Resource.Id.sidone_button);

            if (isFirstImage)
            {
                Bitmap bitmap = ((BitmapDrawable)facilityPhoto.Drawable).Bitmap;
                if (bitmap != null)
                {
                    iImageViewer.SetImageBitmap(bitmap);
                }
            }
            else
            {
                Bitmap bitmap = ((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap;
                if (bitmap != null)
                {
                    iImageViewer.SetImageBitmap(bitmap);
                }
            }

            takeaphotoButton.Click += TakeAPicture;
            selectPictureButton.Click += SelectAPicture;
            cancelButton.Click += siCancelButton_Click;
            doneButton.Click += siDoneButton_Click;
            sameImageDialog = imageDialog;
            imageDialog.Show();
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _File = new Java.IO.File(CreateDirectoryForPictures(), String.Format("{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_File));
            StartActivityForResult(intent, TakeImageId);
        }

        private void SelectAPicture(object sender, EventArgs eventArgs)
        {
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
                SendBroadcast(mediaScanIntent);
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, contentUri);
                iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageHeight(bitmap.Height), false));
            }
            else
            {
                if (data != null)
                {
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, data.Data);
                    iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageHeight(bitmap.Height), false));
                }

            }
        }

        private void siCancelButton_Click(object sender, EventArgs eventArgs)
        {
            imageDialog.Dismiss();
        }

        private void siDoneButton_Click(object sender, EventArgs eventArgs)
        {
            if (IsFirstPhoto)
            {
                Bitmap bitmap = ((BitmapDrawable)iImageViewer.Drawable).Bitmap;
                if (bitmap != null)
                {
                    facilityPhoto.SetImageBitmap(bitmap);
                }
                FirstPhotoIsChanged = true;
            }
            else
            {
                Bitmap bitmap = ((BitmapDrawable)iImageViewer.Drawable).Bitmap;
                if (bitmap != null)
                {
                    secondFacilityPhoto.SetImageBitmap(bitmap);
                }
                SecondPhotoIsChanged = true;
            }
            imageDialog.Dismiss();
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