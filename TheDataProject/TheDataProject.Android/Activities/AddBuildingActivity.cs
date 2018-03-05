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
using System.Collections.Generic;
using Java.Util;

namespace TheDataProject.Droid
{
    [Activity(Label = "AddItemActivity", 
        AlwaysRetainTaskState = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddBuildingActivity : BaseActivity
    {
        FloatingActionButton saveButton, gpscAddLocationButton;
        EditText title, description, occupationYear, buildingName, utilisationStatus, disabledComment, nooOfFoors, totalFootprintAream2, totalImprovedaAeam2, constructionDescription;
        TextView tvbLatitude, tvbLongitude, accuracyMessage;
        ImageView buildingPhoto, iImageViewer;
        NumberPicker numberPicker;
        Button takeaphotoButton, selectPictureButton, siCancelButton, siDoneButton;
        Dialog imageDialog;
        Spinner buildingType, buildingstandard, disabledAccesss;
        AlertDialog numberPickerAlertDialog;
        TextInputLayout occupationyearLayout;
        LinearLayout locationLinearlayout;
        public static readonly int TakeImageId = 1000;
        public static readonly int SelectImageId = 2000;
        Switch heritage;
        public GPSCoordinate _GPSCoordinates { get; set; }
        public BuildingsViewModel ViewModel { get; set; }
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
        public bool isFromCamera = false;
        public bool isEdit = false;
        public bool PhotoIsChanged = false;
        int facilityId;
        int userId;
        string FileName = "";
        Building building;

        protected override int LayoutResource => Resource.Layout.activity_add_building;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            building = new Building();
            ViewModel = new BuildingsViewModel();
            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
            facilityId = Convert.ToInt32(ap.GetFacilityId());
            userId = Convert.ToInt32(ap.GetUserId());

            var data = Intent.GetStringExtra("data");
            // Create your application here

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
            accuracyMessage = FindViewById<TextView>(Resource.Id.accuracy_message);

            _dir = ap.CreateDirectoryForPictures();
            Android.Content.Res.ColorStateList csl = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor("#008000") }); gpscAddLocationButton.BackgroundTintList = csl;
            locationLinearlayout.Visibility = ViewStates.Gone;

            if (data != null)
            {
                building = Newtonsoft.Json.JsonConvert.DeserializeObject<Building>(data);
                isEdit = true;
                SupportActionBar.Title = "Edit Building";
                occupationYear.Text = building.OccupationYear;
                if (building.GPSCoordinates != null)
                {
                    tvbLatitude.Text = "Lat: " + building.GPSCoordinates.Latitude;
                    tvbLongitude.Text = "Long: " + building.GPSCoordinates.Longitude;
                    _GPSCoordinates = building.GPSCoordinates;
                    locationLinearlayout.Visibility = ViewStates.Visible;
                }                
                buildingName.Text = building.BuildingName;
                buildingType.SetSelection(GetIndex(buildingType, building.BuildingType));
                buildingstandard.SetSelection(GetIndex(buildingstandard, building.BuildingStandard));
                disabledAccesss.SetSelection(GetIndex(disabledAccesss, building.DisabledAccess));
                utilisationStatus.Text = building.Status;
                nooOfFoors.Text = Convert.ToString(building.NumberOfFloors);
                totalFootprintAream2.Text = Convert.ToString(building.FootPrintArea);
                totalImprovedaAeam2.Text = Convert.ToString(building.ImprovedArea);
                heritage.Checked = building.Heritage;
                disabledComment.Text = building.DisabledComment;
                constructionDescription.Text = building.ConstructionDescription;

                Bitmap bit = SetImageBitmap(_dir + "/" + building.Photo);
                if (bit != null)
                    buildingPhoto.SetImageBitmap(bit);
                else if (bit == null && !String.IsNullOrEmpty(building.Photo))
                {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Models.Picture picture = await pictureViewModel.ExecuteGetPictureCommand(building.Photo);
                    if (picture != null)
                    {
                        var _bit = ap.StringToBitMap(picture.File);
                        if (_bit != null)
                            SaveImage(_bit, building.Photo);
                        buildingPhoto.SetImageBitmap(_bit);
                    }
                }

            }
            else
            {
                SupportActionBar.Title = "Add New Building";
            }
            saveButton.Click += SaveButton_Click;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            gpscAddLocationButton.Click += AddLocation_Click;
            buildingPhoto.Click += (sender, e) => { ShowImage_Click(); };
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

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
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

            if (requestCode == TakeImageId && resultCode != 0)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_file);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, contentUri);
                iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageHeight(bitmap.Height), false));
            }
            else
            {
                if (data != null)
                {
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, data.Data);
                    iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageHeight(bitmap.Height), false));
                }
            }
        }

        private int GetIndex(Spinner spinner, String myString)
        {
            int index = 0;
            for (int i = 0; i < spinner.Count; i++)
            {
                if (spinner.GetItemAtPosition(i).Equals(myString))
                {
                    index = i;
                }
            }
            return index;
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
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();

            if (!ValidateForm())
            {
                messageDialog.HideLoading();
                return;
            }
            
               
            StaticData staticData = new StaticData();

            int numberOfFloors = 0;
            if (!String.IsNullOrEmpty(nooOfFoors.Text))
                numberOfFloors = Convert.ToInt32(nooOfFoors.Text);
            
            if (PhotoIsChanged)
                FileName = SaveImage(((BitmapDrawable)buildingPhoto.Drawable).Bitmap);

            Building item = new Building
            {
                Id = building.Id,
                BuildingName = buildingName.Text,
                BuildingNumber = building.Id == 0 ? facilityId + staticData.RandomDigits(10) : building.BuildingNumber,
                BuildingType = buildingType.SelectedItem.ToString(),
                BuildingStandard = buildingstandard.SelectedItem.ToString(),
                Status = utilisationStatus.Text,
                NumberOfFloors = numberOfFloors,
                FootPrintArea = Convert.ToDouble(totalFootprintAream2.Text),
                ImprovedArea = Convert.ToDouble(totalImprovedaAeam2.Text),
                Heritage = heritage.Checked == true ? true :false,
                OccupationYear = occupationYear.Text,
                DisabledAccess = disabledAccesss.SelectedItem.ToString(),
                DisabledComment = disabledComment.Text,
                ConstructionDescription = constructionDescription.Text,
                GPSCoordinates = _GPSCoordinates,
                Photo = PhotoIsChanged == true ? FileName : building.Photo,
                CreatedDate = new DateTime(),
                CreatedUserId = userId,
                ModifiedUserId = userId,
                Facility = new Facility
                {
                    Id = facilityId
                }
            };
            bool isAdded = false;
            if (!isEdit)
                isAdded = await ViewModel.AddBuildingAsync(item);
            else
            {
                isAdded = await ViewModel.UpdateBuildingAsync(item);
            }
                
            if (isAdded)
            {
                if (PhotoIsChanged) {
                    PictureViewModel pictureViewModel = new PictureViewModel();
                    Bitmap _bm = ((BitmapDrawable)buildingPhoto.Drawable).Bitmap;
                    string bal = "";
                    if (_bm != null)
                    {
                        MemoryStream stream = new MemoryStream();
                        _bm.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        byte[] ba = stream.ToArray();
                         bal = Base64.EncodeToString(ba, Base64.Default);
                    }

                    Models.Picture picture = new Models.Picture()
                    {
                        Name = FileName,
                        File = bal,
                    };
                    List<Models.Picture> pictures = new List<Models.Picture>();
                    pictures.Add(picture);
                    await pictureViewModel.ExecuteSavePictureCommand(pictures);
                
            
                }
                messageDialog.HideLoading();
                if (!isEdit)
                    messageDialog.SendToast("Building is added successful.");
                else
                    messageDialog.SendToast("Building is updated successful.");
                Finish();
            }
            else {
                messageDialog.HideLoading();
                if (!isEdit)
                    messageDialog.SendToast("Unable to add new building.");
                else
                    messageDialog.SendToast("Unable to update building.");
            }            
        }

        void AddLocation_Click(object sender, EventArgs e)
        {
            
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
           
            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
            else {
                locationLinearlayout.Visibility = ViewStates.Visible;
                tvbLatitude.Text = "Lat: " + location.Latitude.ToString();
                tvbLongitude.Text = "Long: " + location.Longitude.ToString();
                accuracyMessage.Text = String.Format("Accurate to {0} Meters", location.Accuracy.ToString());
                _GPSCoordinates = new GPSCoordinate()
                {
                    Latitude = location.Latitude.ToString(),
                    Longitude = location.Longitude.ToString()
                };

                building.GPSCoordinates = _GPSCoordinates;
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
            numberPicker.MaxValue = new DateTime().Year;
            numberPicker.MinValue = 1900;
            numberPicker.SetGravity(GravityFlags.Center);
            numberPicker.WrapSelectorWheel = true;
            numberPicker.SetScrollContainer(true);
            numberPickerAlertDialog = new AlertDialog.Builder(this).SetTitle("Occupation Year").SetView(npView)
                .SetPositiveButton(Resource.String.done, delegate
                {
                    occupationYear.Text = numberPicker.Value.ToString();
                    numberPickerAlertDialog.Dismiss();
                }).SetNegativeButton(Resource.String.cancel, delegate
                {
                    numberPickerAlertDialog.Dismiss();
                }).Create();
            numberPickerAlertDialog.Show();
        }

        #region Image 
        
        public void SaveImage(Bitmap bitmap,string fileName)
        {
            try
            {
                fileName = String.Format(fileName);
                using (var os = new FileStream(_dir + "/" + fileName, FileMode.CreateNew))
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
            string fileName;
            try
            {
                fileName = String.Format("building_{0}", Guid.NewGuid());
                using (var os = new FileStream(_dir+"/"+fileName, FileMode.CreateNew))
                {
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 95, os);
                }
                return fileName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public void ShowImage_Click()
        {

            imageDialog = new Dialog(this);
            imageDialog.SetContentView(Resource.Layout.dialog_select_image);            
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
            building.Photo = "new Image";
            PhotoIsChanged = true;
        }       
        private bool ValidateForm()
        {
            Validations validation = new Validations();
            MessageDialog messageDialog = new MessageDialog();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            bool isValid = true;
            bool photoIsRequired = false;
            bool GPSCoordinatesIsRequired = false;
                        
            if (!validation.IsRequired(buildingName.Text))
            {
                buildingName.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(utilisationStatus.Text))
            {
                utilisationStatus.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(constructionDescription.Text))
            {
                constructionDescription.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(totalFootprintAream2.Text))
            {
                totalFootprintAream2.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(totalImprovedaAeam2.Text))
            {
                totalImprovedaAeam2.SetError("This field is required", icon);
                isValid = false;
            }
            return isValid;
        }
        #endregion #endregion 
    }
}
