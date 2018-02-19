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
using Android.Provider;
using Android.Graphics;
using System.IO;
using Android.Util;
using Android.Graphics.Drawables;

namespace TheDataProject.Droid
{
    [Activity(Label = "AddItemActivity", LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddBuildingActivity : BaseActivity
    {
        FloatingActionButton saveButton, gpscAddLocationButton;
        EditText title, description, occupationYear, buildingName, utilisationStatus, disabledComment, nooOfFoors, totalFootprintAream2, totalImprovedaAeam2, constructionDescription;
        TextView tvbLatitude, tvbLongitude;
        ImageView buildingPhoto, iImageViewer;
        NumberPicker numberPicker;
        Button takeaphotoButton, selectPictureButton, siCancelButton, siDoneButton;
        Dialog imageDialog;
        Spinner buildingType, buildingstandard, disabledAccesss;
        AlertDialog numberPickerAlertDialog;
        TextInputLayout occupationyearLayout;
        LinearLayout locationLinearlayout;
        Switch heritage;
        public GPSCoordinate _GPSCoordinates { get; set; }
        public BuildingsViewModel ViewModel { get; set; }
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
        public bool isFromCamera = false;
        public bool isEdit = false;
        int facilityId;
        Building building;
        public static readonly int PickImageId = 1000;

        protected override int LayoutResource => Resource.Layout.activity_add_building;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            building = new Building();
            ViewModel = new BuildingsViewModel();
            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
            facilityId = Convert.ToInt32(ap.GetFacilityId());

            var data = Intent.GetStringExtra("data");

            if (data != null)
            {
                building = Newtonsoft.Json.JsonConvert.DeserializeObject<Building>(data);
                isEdit = true;
                SupportActionBar.Title = "Edit Building";
            }
            else
            {
                SupportActionBar.Title = "Add New Building";
            }// Create your application here

            saveButton = FindViewById<FloatingActionButton>(Resource.Id.save_button);
            occupationYear = FindViewById<EditText>(Resource.Id.etb_occupationyear);
            occupationyearLayout = FindViewById<TextInputLayout>(Resource.Id.occupationyear_layout);
            gpscAddLocationButton = FindViewById<FloatingActionButton>(Resource.Id.gpscaddlocation_button);
            locationLinearlayout = FindViewById<LinearLayout>(Resource.Id.blocation_linearlayout);
            tvbLatitude = FindViewById<TextView>(Resource.Id.tvb_latitude);
            tvbLongitude = FindViewById<TextView>(Resource.Id.tvb_longitude);
            buildingPhoto = FindViewById<ImageView>(Resource.Id.imgb_buildingphoto);
            _GPSCoordinates = new GPSCoordinate();
            buildingName = FindViewById<EditText>(Resource.Id.etb_name);
            buildingType = FindViewById<Spinner>(Resource.Id.sf_buildingtype);
            buildingstandard = FindViewById<Spinner>(Resource.Id.sf_buildingstandard);
            utilisationStatus = FindViewById<EditText>(Resource.Id.etb_utilisationstatus);
            nooOfFoors = FindViewById<EditText>(Resource.Id.etb_nooffloors);
            totalFootprintAream2 = FindViewById<EditText>(Resource.Id.etb_totalfootprintaream2);
            totalImprovedaAeam2 = FindViewById<EditText>(Resource.Id.etb_totalimprovedaream2);
            heritage = FindViewById<Switch>(Resource.Id.sf_heritage);
            disabledAccesss = FindViewById<Spinner>(Resource.Id.sf_disabledaccesss);
            disabledComment = FindViewById<EditText>(Resource.Id.etb_disabledcomment);
            constructionDescription = FindViewById<EditText>(Resource.Id.etb_constructiondescription);

            locationLinearlayout.Visibility = ViewStates.Gone;
            gpscAddLocationButton.SetBackgroundColor(Android.Graphics.Color.Green);
            occupationYear.Click += (sender, e) => {
                show();
            };
            saveButton.Click += SaveButton_Click;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            gpscAddLocationButton.Click += AddLocation_Click;
            buildingPhoto.Click += (sender, e) => { ShowImage_Click(); };
        }

        void OnOccupationYearSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            occupationYear.Text = e.Date.ToLongDateString();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Android.Resource.Id.Home)
                return base.OnOptionsItemSelected(item);
            Finish();
            return true;
        }

        async void SaveButton_Click(object sender, EventArgs e)
        {
            StaticData staticData = new StaticData();
            Building item = new Building
            {
                Name = buildingName.Text,
                BuildingNumber = staticData.RandomDigits(10),
                BuildingType = buildingType.SelectedItem.ToString(),
                BuildingStandard = buildingstandard.SelectedItem.ToString(),
                Status = utilisationStatus.Text,
                NumberOfFloors = Convert.ToInt32(nooOfFoors.Text),
                FootPrintArea = Convert.ToDouble(totalFootprintAream2.Text),
                ImprovedArea = Convert.ToDouble(totalImprovedaAeam2.Text),
                Heritage = heritage.Selected.ToString(),
                OccupationYear = occupationYear.Text,
                //DisabledAccess = disabledAccesss.SelectedItem.ToString(),
                DisabledAccess = true,
                DisabledComment = disabledComment.Text,
                ConstructionDescription = constructionDescription.Text,
                GPSCoordinates = _GPSCoordinates,
                Photo = building.Photo,
                CreatedDate = new DateTime(),
                CreatedUserId = Convert.ToInt32(1),
                Facility = new Facility
                {
                    Id = facilityId
                }
            };
            bool isAdded = await ViewModel.AddBuildingAsync(item);
            MessageDialog messageDialog = new MessageDialog();
            if (isAdded)
            {
                messageDialog.SendToast("Building is added successful.");
                Finish();
            }
            else {
                messageDialog.SendMessage("Unable to add new building",null);
            }            
        }

        void AddLocation_Click(object sender, EventArgs e)
        {
            locationLinearlayout.Visibility = ViewStates.Visible;
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
            tvbLatitude.Text = "Latitude: " + location.Latitude.ToString();
            tvbLongitude.Text = "Longitude: " + location.Longitude.ToString();
            _GPSCoordinates = new GPSCoordinate()
            {
                Latitude = location.Latitude.ToString(),
                Longitude = location.Longitude.ToString()
            };

            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
        }

        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
        }

        void NumberPickerCancelButton_Click(object sender, EventArgs e)
        {

        }



        public void show()
        {
            LayoutInflater inflater = (LayoutInflater)Application.Context.GetSystemService(Context.LayoutInflaterService);
            View npView = inflater.Inflate(Resource.Layout.dialog_numberpicker, null);
            numberPicker = npView.FindViewById<NumberPicker>(Resource.Id.numberPicker1);
            numberPicker.MaxValue = 2200;
            numberPicker.MinValue = 1950;
            numberPicker.SetGravity(GravityFlags.Center);
            //numberPicker.SetBackgroundColor(Android.Graphics.Color.LightGray);
            numberPicker.WrapSelectorWheel = true;
            numberPicker.SetScrollContainer(true);
            numberPickerAlertDialog = new AlertDialog.Builder(this).SetTitle("Occupation Year").SetView(npView)
                .SetPositiveButton(Resource.String.done, delegate
                {
                    occupationYear.Text = numberPicker.Value.ToString();
                    numberPickerAlertDialog.Dismiss();
                    numberPickerAlertDialog.Dismiss();
                }).SetNegativeButton(Resource.String.cancel, delegate
                {

                }).Create();
            numberPickerAlertDialog.Show();
        }

        #region Image 
        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            StartActivityForResult(intent, 0);
            isFromCamera = true;
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (isFromCamera)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_file);
                mediaScanIntent.SetData(contentUri);
                Application.Context.SendBroadcast(mediaScanIntent);
                int height = Resources.DisplayMetrics.HeightPixels;
                int width = iImageViewer.Height;
                bitmap = _file.Path.LoadAndResizeBitmap(width, height);
                if (bitmap != null)
                {
                    iImageViewer.SetImageBitmap(bitmap);
                    MemoryStream stream = new MemoryStream();
                    bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
                    byte[] byteArray = stream.ToArray();
                    building.Photo = Base64.EncodeToString(byteArray, 0);
                    bitmap = null;
                }
            }
            else
            {
                Android.Net.Uri uri = data.Data;
                iImageViewer.SetImageURI(uri);
                iImageViewer.DrawingCacheEnabled = true;
                iImageViewer.BuildDrawingCache();
                Android.Graphics.Bitmap bm = iImageViewer.GetDrawingCache(true);
                MemoryStream stream = new MemoryStream();
                bm.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
                byte[] byteArray = stream.ToArray();
                building.Photo = Base64.EncodeToString(byteArray, 0);
            }

            GC.Collect();
        }

        private void CreateDirectoryForPictures()
        {
            _dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
            if (!_dir.Exists())
            {
                _dir.Mkdirs();
            }
        }

        public void ShowImage_Click()
        {

            imageDialog = new Dialog(this);
            imageDialog.SetContentView(Resource.Layout.dialog_select_image);
            CreateDirectoryForPictures();
            takeaphotoButton = imageDialog.FindViewById<Button>(Resource.Id.img_takeaphoto);
            iImageViewer = imageDialog.FindViewById<ImageView>(Resource.Id.imgsi_facilityphoto);
            Bitmap bitmap = ((BitmapDrawable)buildingPhoto.Drawable).Bitmap;
            if (bitmap != null)
            {
                iImageViewer.SetImageBitmap(bitmap);
            }
            selectPictureButton = imageDialog.FindViewById<Button>(Resource.Id.img_selectpicture);
            siCancelButton = imageDialog.FindViewById<Button>(Resource.Id.sicancel_button);
            siDoneButton = imageDialog.FindViewById<Button>(Resource.Id.sidone_button);
            takeaphotoButton.Click += TakeAPicture;
            selectPictureButton.Click += SelectAPicture;
            siCancelButton.Click += siCancelButton_Click;
            siDoneButton.Click += siDoneButton_Click;
            imageDialog.Show();
        }

        private void siCancelButton_Click(object sender, EventArgs eventArgs)
        {
            imageDialog.Dismiss();
        }

        private void siDoneButton_Click(object sender, EventArgs eventArgs)
        {
            Bitmap bitmap = ((BitmapDrawable)iImageViewer.Drawable).Bitmap;
            if (bitmap != null)
            {
                buildingPhoto.SetImageBitmap(bitmap);
            }
            imageDialog.Dismiss();
        }

        private void SelectAPicture(object sender, EventArgs eventArgs)
        {
            Intent Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
            isFromCamera = false;
        }

        #endregion #endregion 
    }
}
