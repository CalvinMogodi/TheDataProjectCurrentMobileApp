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
using Android.Provider;
using Android.Support.Design.Widget;
using TheDataProject.Models;
using Android.Support.V7.Widget;
using TheDataProject.Droid.Helpers;
using TheDataProject.ViewModels;
using System.IO;
using Android.Util;
using static Android.Widget.AdapterView;

namespace TheDataProject.Droid.Activities
{
    [Activity(Label = "ForgotPasswordActivity")]
    public class FacilityInformationActivity : BaseActivity
    {
        public string Photo { get; set; }
        FloatingActionButton editButton, saveButton, gpsLocationButton, bpLocationButton;
        Button locationCancelButton, locationDoneButton, responsiblePersonCancelButton, responsiblePersonDoneButton, deedCancelButton, deedDoneButton
            , takeaphotoButton, selectPictureButton, siCancelButton, siDoneButton;

        EditText streetAddress, suburb, region, fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        TextView clientCode, facilityName, tvfLatitude, tvfLongitude, boundaryPolygonsText;
        ImageView facilityPhoto, iImageViewer, secondFacilityPhoto;
        LinearLayout locationlinearlayout;
        int userId;
        ListView bpListView;
        List<string> itemList, imageNames;
        Dialog imageDialog, sameImageDialog;
        public GPSCoordinate _GPSCoordinates;
        public List<BoundryPolygon> _BoundryPolygons;
        ArrayAdapter<string> arrayAdapter;
        public static readonly int TakeImageId = 1000;
        public static readonly int SelectImageId = 2000;
        int oldPosition;
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmapnew;
        public bool IsFirstPhoto = false;
        public bool FirstPhotoIsChanged = false;
        public bool SecondPhotoIsChanged = false;
        public bool isEdit = false;
        public Java.IO.File _PhotoFile;
        FacilitiesViewModel ViewModel;
        public Facility facility;
        Button buildingsButton;

        protected override int LayoutResource => Resource.Layout.activity_facility;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ViewModel = new FacilitiesViewModel();

            var data = Intent.GetStringExtra("data");           
            
            buildingsButton = FindViewById<Button>(Resource.Id.buildings_button);
            buildingsButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(BuildingsActivity));
                StartActivity(intent);
            };

            Toolbar.MenuItemClick += (sender, e) =>
            {
                var intent = new Intent(this, typeof(LoginActivity));
                AppPreferences appPreferences = new AppPreferences(Application.Context);
                appPreferences.SaveUserId("0");
                StartActivity(intent);
            };
            
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            editButton = FindViewById<FloatingActionButton>(Resource.Id.editfacilityinfo_button);
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.savefacilityinfo_button);
            clientCode = FindViewById<TextView>(Resource.Id.tvf_clientcode);
            facilityName = FindViewById<TextView>(Resource.Id.tvf_facilityname);
            settlementtype = FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            zoning = FindViewById<Spinner>(Resource.Id.sf_zoning);
            locationHolder = FindViewById<CardView>(Resource.Id.tvf_locationholder);
            responsiblepersonHolder = FindViewById<CardView>(Resource.Id.tvf_responsiblepersonholder);
            deedHolder = FindViewById<CardView>(Resource.Id.tvf_deedholder);
            facilityPhoto = FindViewById<ImageView>(Resource.Id.facilityphotoimageinfo);
            secondFacilityPhoto = FindViewById<ImageView>(Resource.Id.facilitysecondphotoinfo);

            facilityPhoto.Click += (sender, e) => {
                ShowImage_Click(true);
            };

            secondFacilityPhoto.Click += (sender, e) => {
                ShowImage_Click(false);
            };
            facility = new Facility();

            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
            userId = Convert.ToInt32(ap.GetUserId());

            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                SupportActionBar.Title = facility.Name;
                clientCode.Text = facility.ClientCode;
                facilityName.Text = facility.Name;
                settlementtype.SetSelection(GetIndex(settlementtype, facility.SettlementType));
                zoning.SetSelection(GetIndex(zoning, facility.Zoning));
                imageNames = facility.IDPicture == null ? new List<string>() : facility.IDPicture.Split(',').ToList();
                if (imageNames.Count > 0)
                    GetImages(ap);
            }
            settlementtype.Enabled = false;
            zoning.Enabled = false;
            saveButton.Visibility = ViewStates.Gone;
            editButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            saveButton.SetBackgroundColor(Android.Graphics.Color.Tan);
            editButton.Click += EditButton_Click;
            saveButton.Click += SaveButton_Click;
            locationHolder.Click += Location_Click;
            responsiblepersonHolder.Click += ResponsiblePerson_Click;
            deedHolder.Click += Deed_Click;
            _GPSCoordinates = new GPSCoordinate();
            _BoundryPolygons = new List<BoundryPolygon>();
        }     

        private void Location_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            // Get the layout inflater
            LayoutInflater inflater = this.LayoutInflater;
            builder.SetView(inflater.Inflate(Resource.Layout.dialog_facility_information_location, null));
            locationDialog = builder.Create();

            locationDialog.Show();
            locationDialog.SetCanceledOnTouchOutside(false);
            int hafh = Resources.DisplayMetrics.HeightPixels / 2;
            int quater = hafh / 2;
            int height = hafh + quater;
            int width = Resources.DisplayMetrics.WidthPixels;
            locationDialog.Window.SetLayout(width, height);
            InitializeLocation(locationDialog);
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
                            SaveImage(_bit, imageNames[0]);
                        facilityPhoto.SetImageBitmap(_bit);
                    }
                }
            }
            if (imageNames.Count == 2)
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
                            SaveImage(_bit, imageNames[1]);
                        secondFacilityPhoto.SetImageBitmap(_bit);
                    }
                }
            }
        }

        void EditButton_Click(object sender, EventArgs e)
        {
            editButton.Visibility = ViewStates.Gone;
            saveButton.Visibility = ViewStates.Visible;
            isEdit = true;
            settlementtype.Enabled = true;
            zoning.Enabled = true;
        }

        async void SaveButton_Click(object sender, EventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog();
            messageDialog.ShowLoading();

            facility.SettlementType = settlementtype.SelectedItem.ToString();
            facility.Zoning = zoning.SelectedItem.ToString();
            if (FirstPhotoIsChanged)
            {
                var _fileName = String.Format("facility_{0}", Guid.NewGuid());
                SaveImage(((BitmapDrawable)facilityPhoto.Drawable).Bitmap, _fileName);
                imageNames.Add(_fileName);
            }
            if (SecondPhotoIsChanged)
            {
                var _fileName = String.Format("facility_{0}", Guid.NewGuid());
                SaveImage(((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap, _fileName);
                imageNames.Add(_fileName);
            }
            if (FirstPhotoIsChanged || SecondPhotoIsChanged)
            {
                facility.IDPicture = "";

                foreach (var name in imageNames)
                {
                    if (String.IsNullOrEmpty(facility.IDPicture))
                        facility.IDPicture = name;
                    else
                        facility.IDPicture = facility.IDPicture + "," + name;
                }
            }

            facility.ModifiedUserId = userId;
            facility.ModifiedDate = new DateTime();
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

                editButton.Visibility = ViewStates.Visible;
                saveButton.Visibility = ViewStates.Gone;
                isEdit = false;
                settlementtype.Enabled = false;
                zoning.Enabled = false;
                messageDialog.HideLoading();
                messageDialog.SendToast("Facility is updated successful.");
            }
            else
            {
                messageDialog.HideLoading();
                messageDialog.SendToast("Facility is not updated successful.");
            }
            this.isEdit = false;
        }

        public void SaveImage(Bitmap bitmap, string fileName)
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

        public void ShowImage_Click(bool isFirstImage)
        {
            IsFirstPhoto = isFirstImage;
            imageDialog = new Dialog(this);

            imageDialog.SetContentView(Resource.Layout.dialog_select_image);
            takeaphotoButton = imageDialog.FindViewById<Button>(Resource.Id.img_takeaphoto);
            iImageViewer = imageDialog.FindViewById<ImageView>(Resource.Id.imgsi_facilityphoto);
            selectPictureButton = imageDialog.FindViewById<Button>(Resource.Id.img_selectpicture);
            siCancelButton = imageDialog.FindViewById<Button>(Resource.Id.sicancel_button);
            siDoneButton = imageDialog.FindViewById<Button>(Resource.Id.sidone_button);

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
            siCancelButton.Click += siCancelButton_Click;
            siDoneButton.Click += siDoneButton_Click;
            sameImageDialog = imageDialog;
            imageDialog.Show();
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _PhotoFile = new Java.IO.File(CreateDirectoryForPictures(), String.Format("{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_PhotoFile));
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

            if (requestCode == TakeImageId && resultCode != 0 && _PhotoFile != null)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

                Android.Net.Uri contentUri = Android.Net.Uri.FromFile(_PhotoFile);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, contentUri);
                iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageWidth(bitmap.Height), false));
            }
            else
            {
                if (data != null)
                {
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, data.Data);
                    iImageViewer.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, ap.GetImageWidth(bitmap.Width), ap.GetImageWidth(bitmap.Height), false));
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

        #region Location 
        private void InitializeLocation(AlertDialog dialog)
        {
            streetAddress = dialog.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = dialog.FindViewById<EditText>(Resource.Id.etf_suburb);
            region = dialog.FindViewById<EditText>(Resource.Id.etf_region);
            province = dialog.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = dialog.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            locationCancelButton = dialog.FindViewById<Button>(Resource.Id.dfil_cancelbutton);
            locationDoneButton = dialog.FindViewById<Button>(Resource.Id.dfil_donebutton);
            gpsLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.gpscaddlocation_button);
            bpLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.bpaddlocation_button);
            locationlinearlayout = dialog.FindViewById<LinearLayout>(Resource.Id.location_linearlayout);
            tvfLatitude = dialog.FindViewById<TextView>(Resource.Id.tvf_latitude);
            tvfLongitude = dialog.FindViewById<TextView>(Resource.Id.tvf_longitude);
            boundaryPolygonsText = dialog.FindViewById<TextView>(Resource.Id.boundaryPolygonsText);
            bpListView = dialog.FindViewById<ListView>(Resource.Id.bplistView1);
            locationlinearlayout.Visibility = ViewStates.Gone;
            itemList = new List<string>();

            if (facility.Location != null)
            {
                streetAddress.Text = facility.Location.StreetAddress;
                suburb.Text = facility.Location.Suburb;
                region.Text = facility.Location.Region;
                province.SetSelection(GetIndex(province, facility.Location.Province));
                localmunicipality.SetSelection(GetIndex(localmunicipality, facility.Location.LocalMunicipality));

                if (facility.Location.GPSCoordinates != null)
                {
                    locationlinearlayout.Visibility = ViewStates.Visible;
                    tvfLatitude.Text = "Lat: " + facility.Location.GPSCoordinates.Latitude;
                    tvfLongitude.Text = " Long: " + facility.Location.GPSCoordinates.Longitude;
                }

                if (facility.Location.BoundryPolygon != null)
                {
                    _BoundryPolygons = new List<BoundryPolygon>();
                    foreach (var BoundaryPolygon in facility.Location.BoundryPolygon)
                    {
                        _BoundryPolygons.Add(BoundaryPolygon);
                        itemList.Add("Lat: " + BoundaryPolygon.Latitude.ToString() + " Long: " + BoundaryPolygon.Longitude.ToString());
                    }

                    arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, itemList);
                    bpListView.Adapter = arrayAdapter;
                    bpListView.ItemLongClick += Adapter_ItemSwipe;
                }
                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
            }


            locationCancelButton.Click += LocationCancelButton_Click;
            locationDoneButton.Click += LocationDoneButton_Click;
            bpLocationButton.Click += BPLocationButton_Click;
            gpsLocationButton.Click += GPSLocationButton_Click;

            Android.Content.Res.ColorStateList csl = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor("#008000") }); bpLocationButton.BackgroundTintList = csl;
            Android.Content.Res.ColorStateList cslf = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { Android.Graphics.Color.ParseColor("#008000") }); gpsLocationButton.BackgroundTintList = cslf;

            if (isEdit)
            {
                streetAddress.Enabled = true;
                suburb.Enabled = true;
                region.Enabled = true;
                province.Enabled = true;
                localmunicipality.Enabled = true;
                locationDoneButton.Enabled = true;
                gpsLocationButton.Enabled = true;
                bpLocationButton.Enabled = true;
            }
            else
            {
                streetAddress.Enabled = false;
                suburb.Enabled = false;
                region.Enabled = false;
                province.Enabled = false;
                localmunicipality.Enabled = false;
                locationDoneButton.Enabled = false;
                gpsLocationButton.Enabled = false;
                bpLocationButton.Enabled = false;
            }
        }

        private void BPLocationButton_Click(object sender, EventArgs eventArgs)
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
            else
            {
                BoundryPolygon BoundryPolygon = new BoundryPolygon()
                {
                    Latitude = location.Latitude.ToString(),
                    Longitude = location.Longitude.ToString()
                };
                _BoundryPolygons.Add(BoundryPolygon);
                itemList.Add("Lat: " + location.Latitude.ToString() + " Long: " + location.Longitude.ToString());

                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, itemList);
                bpListView.Adapter = arrayAdapter;
                bpListView.ItemLongClick += Adapter_ItemSwipe;
            }


        }
        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
        }
        void Adapter_ItemSwipe(object sender, ItemLongClickEventArgs e)
        {
            if (oldPosition != e.Position)
            {
                var item = arrayAdapter.GetItem(e.Position);
                arrayAdapter.Remove(item);
                itemList.Remove(item);
                _BoundryPolygons.RemoveAt(e.Position);
                boundaryPolygonsText.Text = String.Format("Boundary Polygons {0}", itemList.Count);
                oldPosition = e.Position;
                bpListView.Adapter = arrayAdapter;
                bpListView.ItemLongClick += Adapter_ItemSwipe;
            }
        }

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
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
            else
            {
                locationlinearlayout.Visibility = ViewStates.Visible;
                tvfLatitude.Text = "Lat: " + location.Latitude.ToString();
                tvfLongitude.Text = "Long: " + location.Longitude.ToString();
            }
        }
        private void LocationCancelButton_Click(object sender, EventArgs e)
        {
            locationDialog.Cancel();
        }
        private bool ValidateLocation()
        {
            Validations validation = new Validations();
            MessageDialog messageDialog = new MessageDialog();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            bool isValid = true;

            if (!validation.IsRequired(streetAddress.Text))
            {
                streetAddress.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(suburb.Text))
            {
                suburb.SetError("This field is required", icon);
                isValid = false;
            }
            if (facility.Location == null)
            {
                if (facility.Location.GPSCoordinates == null)
                {
                    messageDialog.SendToast("Please add an GPS coordinates");
                    isValid = false;
                }

            }
            return isValid;
        }

        private bool ValidateDeedInfo()
        {
            Validations validation = new Validations();
            MessageDialog messageDialog = new MessageDialog();
            Android.Graphics.Drawables.Drawable icon = Resources.GetDrawable(Resource.Drawable.error);
            icon.SetBounds(0, 0, icon.IntrinsicWidth, icon.IntrinsicHeight);

            bool isValid = true;

            if (!validation.IsRequired(erfNumber.Text))
            {
                erfNumber.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(titleDeedNumber.Text))
            {
                titleDeedNumber.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(extentm2.Text))
            {
                extentm2.SetError("This field is required", icon);
                isValid = false;
            }
            if (!validation.IsRequired(ownerInformation.Text))
            {
                ownerInformation.SetError("This field is required", icon);
                isValid = false;
            }
            return isValid;
        }
        private void LocationDoneButton_Click(object sender, EventArgs e)
        {
            if (!ValidateLocation())
                return;

            facility.Location = new Models.Location();
            facility.Location.LocalMunicipality = localmunicipality.SelectedItem.ToString();
            facility.Location.Province = province.SelectedItem.ToString();
            facility.Location.StreetAddress = streetAddress.Text;
            facility.Location.Suburb = suburb.Text;
            facility.Location.Region = region.Text;
            facility.Location.GPSCoordinates = new Models.GPSCoordinate()
            {
                Longitude = tvfLatitude.Text,
                Latitude = tvfLongitude.Text,
            };
            facility.Location.BoundryPolygon = _BoundryPolygons;
            locationDialog.Cancel();
        }
        #endregion #endregion 

        #region Responsible Person 

        private void ResponsiblePerson_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            // Get the layout inflater
            LayoutInflater inflater = this.LayoutInflater;
            builder.SetView(inflater.Inflate(Resource.Layout.dialog_facility_information_responsible_person, null));
            responsiblePersonDialog = builder.Create();

            responsiblePersonDialog.Show();
            responsiblePersonDialog.SetCanceledOnTouchOutside(false);
            InitializeResponsiblePerson(responsiblePersonDialog);
        }

        private void InitializeResponsiblePerson(AlertDialog dialog)
        {
            fullname = dialog.FindViewById<EditText>(Resource.Id.etf_fullname);
            designation = dialog.FindViewById<EditText>(Resource.Id.etf_designation);
            mobileNumber = dialog.FindViewById<EditText>(Resource.Id.etf_mobileNumber);
            emailaddress = dialog.FindViewById<EditText>(Resource.Id.etf_emailaddress);
            responsiblePersonCancelButton = dialog.FindViewById<Button>(Resource.Id.dfirp_cancelbutton);
            responsiblePersonDoneButton = dialog.FindViewById<Button>(Resource.Id.dfirp_donebutton);

            if (facility.ResposiblePerson != null)
            {
                fullname.Text = facility.ResposiblePerson.FullName;
                designation.Text = facility.ResposiblePerson.Designation;
                mobileNumber.Text = facility.ResposiblePerson.PhoneNumber;
                emailaddress.Text = facility.ResposiblePerson.EmailAddress;
            }

            responsiblePersonCancelButton.Click += ResponsiblePersonCancelButton_Click;
            responsiblePersonDoneButton.Click += ResponsiblePersonDoneButton_Click;

            if (isEdit)
            {
                fullname.Enabled = true;
                designation.Enabled = true;
                mobileNumber.Enabled = true;
                emailaddress.Enabled = true;
                responsiblePersonDoneButton.Enabled = true;
            }
            else
            {
                fullname.Enabled = false;
                designation.Enabled = false;
                mobileNumber.Enabled = false;
                emailaddress.Enabled = false;
                responsiblePersonDoneButton.Enabled = false;
            }
        }

        private void ResponsiblePersonCancelButton_Click(object sender, EventArgs e)
        {
            responsiblePersonDialog.Cancel();
        }

        private void ResponsiblePersonDoneButton_Click(object sender, EventArgs e)
        {

            facility.ResposiblePerson = new Models.Person();
            facility.ResposiblePerson.FullName = fullname.Text;
            facility.ResposiblePerson.Designation = designation.Text;
            facility.ResposiblePerson.PhoneNumber = mobileNumber.Text;
            facility.ResposiblePerson.EmailAddress = emailaddress.Text;
            responsiblePersonDialog.Cancel();
        }
        #endregion #endregion 

        #region Deed Office

        private void Deed_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            // Get the layout inflater
            LayoutInflater inflater = this.LayoutInflater;
            builder.SetView(inflater.Inflate(Resource.Layout.dialog_facility_information_deed, null));
            deedDialog = builder.Create();

            deedDialog.Show();
            deedDialog.SetCanceledOnTouchOutside(false);
            InitializeDeed(deedDialog);
        }

        private void InitializeDeed(AlertDialog dialog)
        {
            erfNumber = dialog.FindViewById<EditText>(Resource.Id.etf_erfnumber);
            titleDeedNumber = dialog.FindViewById<EditText>(Resource.Id.etf_titledeednumber);
            extentm2 = dialog.FindViewById<EditText>(Resource.Id.etf_extentm2);
            ownerInformation = dialog.FindViewById<EditText>(Resource.Id.etf_ownerinformation);
            deedCancelButton = dialog.FindViewById<Button>(Resource.Id.dfid_cancelbutton);
            deedDoneButton = dialog.FindViewById<Button>(Resource.Id.dfid_donebutton);

            if (facility.DeedsInfo != null)
            {
                erfNumber.Text = facility.DeedsInfo.ErFNumber;
                titleDeedNumber.Text = facility.DeedsInfo.TitleDeedNumber;
                extentm2.Text = facility.DeedsInfo.Extent;
                ownerInformation.Text = facility.DeedsInfo.OwnerInfomation;

            }
            deedCancelButton.Click += DeedCancelButton_Click;
            deedDoneButton.Click += DeedDoneButton_Click;

            if (isEdit)
            {
                erfNumber.Enabled = true;
                titleDeedNumber.Enabled = true;
                extentm2.Enabled = true;
                ownerInformation.Enabled = true;
                deedDoneButton.Enabled = true;
            }
            else
            {
                erfNumber.Enabled = false;
                titleDeedNumber.Enabled = false;
                extentm2.Enabled = false;
                ownerInformation.Enabled = false;
                deedDoneButton.Enabled = false;
            }
        }

        private void DeedCancelButton_Click(object sender, EventArgs e)
        {
            deedDialog.Cancel();
        }

        private void DeedDoneButton_Click(object sender, EventArgs e)
        {
            if (!ValidateDeedInfo())
                return;
            facility.DeedsInfo = new Models.DeedsInfo();
            facility.DeedsInfo.ErFNumber = erfNumber.Text;
            facility.DeedsInfo.TitleDeedNumber = titleDeedNumber.Text;
            facility.DeedsInfo.Extent = extentm2.Text;
            facility.DeedsInfo.OwnerInfomation = ownerInformation.Text;
            deedDialog.Cancel();
        }
        #endregion #endregion        
    }
}