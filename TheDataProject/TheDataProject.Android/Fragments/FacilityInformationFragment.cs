using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using TheDataProject.Droid.Helpers;
using System.IO;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Locations;
using Android.Provider;
using Android.Content.PM;
using Android.Graphics.Drawables;

namespace TheDataProject.Droid.Fragments
{
    public class FacilityInformationFragment : Android.Support.V4.App.Fragment , IFragmentVisible
    {


        #region Properties 
        public string Photo { get; set; }
        public static readonly int PickImageId = 1000;
        FloatingActionButton editButton, saveButton, gpsLocationButton, bpLocationButton;
        Button locationCancelButton, locationDoneButton, responsiblePersonCancelButton, responsiblePersonDoneButton, deedCancelButton, deedDoneButton
            , takeaphotoButton, selectPictureButton, siCancelButton, siDoneButton;
        ProgressBar progress;
        EditText streetAddress, suburb, fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        LayoutInflater Inflater;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        TextView clientCode, facilityName, tvfLatitude, tvfLongitude;
        ImageView facilityPhoto, iImageViewer;
        LinearLayout locationlinearlayout;
        View view;
        ListView mListView;
        Dialog imageDialog;
        FacilityDetailViewModel facilityDetailViewModel;

        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
        public bool isFromCamera = false;
        public bool isEdit = false;

        public static FacilitiesViewModel ViewModel { get; set; }
        public Facility facility;
        public static FacilityInformationFragment NewInstance(Bundle mybundle) => 
            new FacilityInformationFragment { Arguments = mybundle };
       

        #endregion #endregion 

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewModel = new FacilitiesViewModel();
            
            Inflater = inflater;
            view = inflater.Inflate(Resource.Layout.fragment_facility_information, container, false);
            editButton = view.FindViewById<FloatingActionButton>(Resource.Id.editfacilityinfo_button);
            saveButton = view.FindViewById<FloatingActionButton>(Resource.Id.savefacilityinfo_button);
            clientCode = view.FindViewById<TextView>(Resource.Id.tvf_clientcode);
            facilityName = view.FindViewById<TextView>(Resource.Id.tvf_facilityname);
            settlementtype = view.FindViewById<Spinner>(Resource.Id.sf_settlementtype);
            zoning = view.FindViewById<Spinner>(Resource.Id.sf_zoning);
            locationHolder = view.FindViewById<CardView>(Resource.Id.tvf_locationholder);
            responsiblepersonHolder = view.FindViewById<CardView>(Resource.Id.tvf_responsiblepersonholder);
            deedHolder = view.FindViewById<CardView>(Resource.Id.tvf_deedholder);
            facilityPhoto = view.FindViewById<ImageView>(Resource.Id.imgf_facilityphoto);           
            facilityPhoto.Click += (sender, e) => {
                ShowImage_Click();
            };
            facility = new Facility();

            var data = Arguments.GetString("data");

            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                clientCode.Text = facility.ClientCode;
                facilityName.Text = facility.Name;
                //settlementtype.SelectedItem = facility.SettlementType;
                //zoning.SelectedItem = facility.Zoning;
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

            return view;
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

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            StartActivityForResult(intent, 0);
            isFromCamera = true;
        }

       
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
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
                    facility.IDPicture = Base64.EncodeToString(byteArray, 0);
                    bitmap = null;
                }
            }
            else {
                Android.Net.Uri uri = data.Data;
                iImageViewer.SetImageURI(uri);
                iImageViewer.DrawingCacheEnabled = true;
                iImageViewer.BuildDrawingCache();
                Android.Graphics.Bitmap bm = iImageViewer.GetDrawingCache(true);
                MemoryStream stream = new MemoryStream();
                bm.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
                byte[] byteArray = stream.ToArray();
                facility.IDPicture = Base64.EncodeToString(byteArray, 0);
            }
            
            GC.Collect();
        }
        public void ShowImage_Click()
        {

            imageDialog = new Dialog(Activity);
            imageDialog.SetContentView(Resource.Layout.dialog_select_image);           
            CreateDirectoryForPictures();
            takeaphotoButton = imageDialog.FindViewById<Button>(Resource.Id.img_takeaphoto);
            iImageViewer = imageDialog.FindViewById<ImageView>(Resource.Id.imgsi_facilityphoto);
            Bitmap bitmap = ((BitmapDrawable)facilityPhoto.Drawable).Bitmap;
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
                facilityPhoto.SetImageBitmap(bitmap);
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

        public void BecameVisible()
        {

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

            facility.SettlementType = settlementtype.SelectedItem.ToString();
            facility.Zoning = zoning.SelectedItem.ToString();
            bool isUpdated = await ViewModel.ExecuteUpdateFacilityCommand(facility);
            if (isUpdated)
            {
                editButton.Visibility = ViewStates.Visible;
                saveButton.Visibility = ViewStates.Gone;
            }
            else {

            }
            this.isEdit = false;
        }

        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
        }

        #region Location 
        private void InitializeLocation(AlertDialog dialog)
        {
            streetAddress = dialog.FindViewById<EditText>(Resource.Id.etf_streetAddress);
            suburb = dialog.FindViewById<EditText>(Resource.Id.etf_suburb);
            province = dialog.FindViewById<Spinner>(Resource.Id.sf_province);
            localmunicipality = dialog.FindViewById<Spinner>(Resource.Id.sf_localmunicipality);
            locationCancelButton = dialog.FindViewById<Button>(Resource.Id.dfil_cancelbutton);
            locationDoneButton = dialog.FindViewById<Button>(Resource.Id.dfil_donebutton);
            gpsLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.gpscaddlocation_button);
            bpLocationButton = dialog.FindViewById<FloatingActionButton>(Resource.Id.bpaddlocation_button);
            locationlinearlayout = dialog.FindViewById<LinearLayout>(Resource.Id.location_linearlayout);
            tvfLatitude = dialog.FindViewById<TextView>(Resource.Id.tvf_latitude);
            tvfLongitude = dialog.FindViewById<TextView>(Resource.Id.tvf_longitude);            
            locationlinearlayout.Visibility = ViewStates.Gone;
            
            locationCancelButton.Click += LocationCancelButton_Click;
            locationDoneButton.Click += LocationDoneButton_Click;
            bpLocationButton.Click += BPLocationButton_Click;
            gpsLocationButton.Click += GPSLocationButton_Click;

            if (isEdit)
            {
                streetAddress.Enabled = true;
                suburb.Enabled = true;
                province.Enabled = true;
                localmunicipality.Enabled = true;
                locationDoneButton.Enabled = true;
                gpsLocationButton.Enabled = true;
                bpLocationButton.Enabled = true;
            }
            else {
                streetAddress.Enabled = false;
                suburb.Enabled = false;
                province.Enabled = false;
                localmunicipality.Enabled = false;
                locationDoneButton.Enabled = false;
                gpsLocationButton.Enabled = false;
                bpLocationButton.Enabled = false;
            }
        }


        private void Location_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_location, null));
            locationDialog = builder.Create();

            locationDialog.Show();
            locationDialog.SetCanceledOnTouchOutside(false);
            InitializeLocation(locationDialog);
        }

        private void BPLocationButton_Click(object sender, EventArgs eventArgs)
        {

        }

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
        {
            locationlinearlayout.Visibility = ViewStates.Visible;
            LocationManager locationManager = Application.Context.GetSystemService(Context.LocationService) as LocationManager;

            Location location = null;
            // getting GPS status

            bool isGPSEnabled = locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            bool isNetworkEnabled = locationManager.IsProviderEnabled(LocationManager.NetworkProvider);
            bool isPassiveProviderEnabled = locationManager.IsProviderEnabled(LocationManager.PassiveProvider);

            if (!isGPSEnabled && !isNetworkEnabled && !isPassiveProviderEnabled)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("GPS is not enabled");
                ShowSettingsAlert();
            }
            else
            {
                if (isGPSEnabled)
                {
                    location = locationManager.GetLastKnownLocation(LocationManager.GpsProvider);
                }
                if (isNetworkEnabled)
                {
                    if (location == null)
                        location = locationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
                }
                if (isPassiveProviderEnabled)
                {
                    if (location == null)
                        location = locationManager.GetLastKnownLocation(LocationManager.PassiveProvider);
                }

                tvfLatitude.Text = location.Latitude.ToString();
                tvfLongitude.Text = location.Longitude.ToString();
            }
            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }
        }

        private void LocationCancelButton_Click(object sender, EventArgs e)
        {
            locationDialog.Cancel();
        }

        private void LocationDoneButton_Click(object sender, EventArgs e)
        {
            facility.Location = new Models.Location();
            facility.Location.LocalMunicipality = localmunicipality.SelectedItem.ToString();
            facility.Location.StreetAddress = streetAddress.Text;
            facility.Location.Suburb = suburb.Text;
            facility.Location.Coordinates = new Models.GPSCoordinate() {
                Longitude = tvfLatitude.Text,
                Latitude = tvfLongitude.Text,
            };
            facility.GPSCoordinates = new Models.GPSCoordinate() {
                Longitude = tvfLatitude.Text,
                Latitude = tvfLongitude.Text,
            };
            facility.Location.BoundaryPolygon = new Models.BoundryPolygon()
            {

            };
            locationDialog.Cancel();
        }
        #endregion #endregion 

        #region Responsible Person 

        private void ResponsiblePerson_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_responsible_person, null));
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
            else {
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
            facility.ResposiblePerson.Name = fullname.Text;
            facility.ResposiblePerson.Designation = designation.Text;
            facility.ResposiblePerson.PhoneNumber = mobileNumber.Text;
            facility.ResposiblePerson.EmailAddress = emailaddress.Text;
            responsiblePersonDialog.Cancel();
        }
        #endregion #endregion 

        #region Deed Office

        private void Deed_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            // Get the layout inflater
            builder.SetView(Inflater.Inflate(Resource.Layout.dialog_facility_information_deed, null));
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