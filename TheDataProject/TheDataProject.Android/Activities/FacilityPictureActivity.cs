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
using Android.Provider;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "Facility Picture Activity", AlwaysRetainTaskState = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class FacilityPictureActivity : Android.Support.V7.App.AppCompatActivity
    {
        Button takeaphotoButton, selectPictureButton, selectSecondImgButton, takeaSecondImgButton;
        ImageView facilityPhoto, secondFacilityPhoto;
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
        private AppPreferences appPreferences;
        public Android.Support.V7.Widget.Toolbar Toolbar
        {
            get;
            set;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_facility_picture);
            secondFacilityPhoto = FindViewById<ImageView>(Resource.Id.facility_secondphoto_imageview);
            facilityPhoto = FindViewById<ImageView>(Resource.Id.facility_photo_imageview);
            appPreferences = new AppPreferences(Application.Context);

            takeaphotoButton = FindViewById<Button>(Resource.Id.img_takeaphoto);
            selectPictureButton = FindViewById<Button>(Resource.Id.img_selectpicture);
            selectSecondImgButton = FindViewById<Button>(Resource.Id.secondimg_selectpicture);
            takeaSecondImgButton = FindViewById<Button>(Resource.Id.secondimg_takeaphoto);

            selectPictureButton.Click += SelectAPicture;
            takeaphotoButton.Click += TakeAPicture;
            selectSecondImgButton.Click += SelectASecondPicture;
            takeaSecondImgButton.Click += TakeASecondPicture;

            _dir = CreateDirectoryForPictures();
            Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (Toolbar != null)
            {
                SetSupportActionBar(Toolbar);
                SupportActionBar.Title = "Facility Pictures";
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
            }
            Toolbar.MenuItemClick += (sender, e) =>
            {
                var itemTitle = e.Item.TitleFormatted;
                switch (itemTitle.ToString())
                {
                    case "Log Out":
                        var intent = new Intent(this, typeof(LoginActivity));
                        appPreferences.SaveUserId("0");
                        StartActivity(intent);
                        break;
                    case "Save":
                        SaveFacility();
                        break;
                }
            };
            var data = Intent.GetStringExtra("data");
            facility = new Facility();
            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                imageNames = facility.IDPicture == null ? new List<string>() : facility.IDPicture.Split(',').ToList();
                if (imageNames.Count > 0)
                    GetImages(appPreferences);
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

        private async void SaveFacility()
        {
            if (appPreferences.IsOnline(Application.Context))
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.ShowLoading();
                if (imageNames.Count() == 0)
                {
                    imageNames = new List<string>();
                }

                if (FirstPhotoIsChanged)
                {
                    string thisFileName = appPreferences.SaveImage(((BitmapDrawable)facilityPhoto.Drawable).Bitmap);
                    if (imageNames.Count() > 0)
                        imageNames[0] = thisFileName;
                    else
                        imageNames.Add(thisFileName);

                }
                if (SecondPhotoIsChanged)
                {
                    var _fileName = String.Format("facility_{0}", Guid.NewGuid());
                    appPreferences.SaveImage(((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap, _fileName);
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
                            file = Base64.EncodeToString(ba, Base64Flags.Default);
                        }

                        Models.Picture picture = new Models.Picture()
                        {
                            Name = imageNames[0],
                            File = file,
                        };
                        pictures.Add(picture);
                    }
                    if (SecondPhotoIsChanged && imageNames.Count() > 1)
                    {
                        Bitmap _bm = ((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap;
                        string file = "";
                        if (_bm != null)
                        {
                            MemoryStream stream = new MemoryStream();
                            _bm.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                            byte[] ba = stream.ToArray();
                            file = Base64.EncodeToString(ba, Base64Flags.Default);
                        }

                        Models.Picture picture = new Models.Picture()
                        {
                            Name = imageNames[1],
                            File = file,
                        };
                        pictures.Add(picture);
                    }
                    bool isSuccess = await pictureViewModel.ExecuteSavePictureCommand(pictures);

                    messageDialog.HideLoading();
                    messageDialog.SendToast("Pictures are saved successful.");
                    var intent = new Intent(this, typeof(FacilityDetailActivity));
                    Context mContext = Android.App.Application.Context;
                    AppPreferences ap = new AppPreferences(mContext);
                    ap.SaveFacilityId(facility.Id.ToString());
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
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            for (int j = 0; j < menu.Size(); j++)
            {
                var item = menu.GetItem(j);
                if (item.ToString() == "Search")
                    item.SetVisible(false);
                if (item.ToString() == "Submit")
                    item.SetVisible(false);
                if (item.ToString() == "Add")
                    item.SetVisible(false);
                if (item.ToString() == "Save")
                    item.SetShowAsActionFlags(Android.Views.ShowAsAction.Always);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        async void TakeAPicture(object sender, EventArgs e)
        {
            IsFirstPhoto = true;
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            _File = new Java.IO.File(CreateDirectoryForPictures(), String.Format("{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_File));
            StartActivityForResult(intent, TakeImageId);
        }

        void TakeASecondPicture(object sender, EventArgs e)
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

            if (requestCode == TakeImageId && resultCode != 0 && _File != null)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_File);
                mediaScanIntent.SetData(contentUri);
                Application.Context.SendBroadcast(mediaScanIntent);
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, contentUri);
                Bitmap formattedBitmap = Bitmap.CreateScaledBitmap(bitmap, appPreferences.GetImageWidth(bitmap.Width), appPreferences.GetImageHeight(bitmap.Height), false);
                if (formattedBitmap != null)
                {
                    if (IsFirstPhoto)
                    {
                        facilityPhoto.SetImageBitmap(formattedBitmap);
                        FirstPhotoIsChanged = true;
                    }
                    else
                    {
                        secondFacilityPhoto.SetImageBitmap(formattedBitmap);
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
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, data.Data);
                    if (IsFirstPhoto)
                    {
                        facilityPhoto.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, appPreferences.GetImageWidth(bitmap.Width), appPreferences.GetImageHeight(bitmap.Height), false));
                        FirstPhotoIsChanged = true;
                    }
                    else
                    {
                        secondFacilityPhoto.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, appPreferences.GetImageWidth(bitmap.Width), appPreferences.GetImageHeight(bitmap.Height), false));
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