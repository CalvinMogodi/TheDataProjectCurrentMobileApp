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
using TheDataProject.Models;
using static Android.Widget.AdapterView;

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
        EditText streetAddress, suburb,region ,fullname, designation, mobileNumber, emailaddress, erfNumber, titleDeedNumber, extentm2, ownerInformation;
        Spinner settlementtype, province, localmunicipality, zoning;
        AlertDialog locationDialog, responsiblePersonDialog, deedDialog;
        LayoutInflater Inflater;
        CardView locationHolder, responsiblepersonHolder, deedHolder;
        TextView clientCode, facilityName, tvfLatitude, tvfLongitude;
        ImageView facilityPhoto, iImageViewer, secondFacilityPhoto;
        LinearLayout locationlinearlayout;
        View view;
        ListView bpListView;
        List<string> itemList, imageNames;
        Dialog imageDialog;
        FacilityDetailViewModel facilityDetailViewModel;
        public GPSCoordinate _GPSCoordinates;
        public List<GPSCoordinate> _BoundryPolygonGPSCoordinates;
        ArrayAdapter<string> arrayAdapter;
        int oldPosition;
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
        public bool IsFirstPhoto = false;
        public bool FirstPhotoIsChanged = false;
        public bool SecondPhotoIsChanged = false;
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
            facilityPhoto = view.FindViewById<ImageView>(Resource.Id.facilityphotoimageinfo);
            secondFacilityPhoto = view.FindViewById<ImageView>(Resource.Id.facilitysecondphoto);

            facilityPhoto.Click += (sender, e) => {
                ShowImage_Click(true);
            };

            secondFacilityPhoto.Click += (sender, e) => {
                ShowImage_Click(false);
            };
            facility = new Facility();

            AppPreferences ap = new AppPreferences(Android.App.Application.Context);
            _dir = ap.CreateDirectoryForPictures();
            var data = Arguments.GetString("data");

            if (data != null)
            {
                facility = Newtonsoft.Json.JsonConvert.DeserializeObject<Facility>(data);
                clientCode.Text = facility.ClientCode;
                facilityName.Text = facility.Name;
                settlementtype.SetSelection(GetIndex(settlementtype, facility.SettlementType));
                zoning.SetSelection(GetIndex(zoning, facility.Zoning));
                imageNames = facility.IDPicture.Split(',').ToList();
                if (!String.IsNullOrEmpty(imageNames[0]))
                {
                    Bitmap bit = ap.SetImageBitmap(_dir + "/" + imageNames[0]);
                    if (bit != null)
                        facilityPhoto.SetImageBitmap(bit);
                }
                if (imageNames.Count == 2)
                {
                    Bitmap bit = ap.SetImageBitmap(_dir + "/" + imageNames[1]);
                    if (bit != null)
                        secondFacilityPhoto.SetImageBitmap(bit);
                }
                
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
            _BoundryPolygonGPSCoordinates = new List<GPSCoordinate>();

            return view;
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

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            _file = new Java.IO.File(_dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(_file));
            isFromCamera = true;
            StartActivityForResult(intent, 0);            
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
                    bitmap = null;
                }
            }
            else
            {
                if (data != null)
                {
                    Android.Net.Uri uri = data.Data;
                    iImageViewer.SetImageURI(uri);
                    bitmap = null;
                }               
            }

            GC.Collect();
        }

        public void ShowImage_Click(bool isFirstImage)
        {
            IsFirstPhoto = isFirstImage;
            imageDialog = new Dialog(Activity);
            imageDialog.SetContentView(Resource.Layout.dialog_select_image);   
            takeaphotoButton = imageDialog.FindViewById<Button>(Resource.Id.img_takeaphoto);
            iImageViewer = imageDialog.FindViewById<ImageView>(Resource.Id.imgsi_facilityphoto);
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

        private void SelectAPicture(object sender, EventArgs eventArgs)
        {
            isFromCamera = false;
            Intent Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
         
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
            imageNames = new List<string>();
            if (FirstPhotoIsChanged)
            {
                string fileName = SaveImage(((BitmapDrawable)facilityPhoto.Drawable).Bitmap);
                imageNames.Add(fileName);
            }
            if (SecondPhotoIsChanged)
            {
                string fileName = SaveImage(((BitmapDrawable)secondFacilityPhoto.Drawable).Bitmap);
                imageNames.Add(fileName);
            }
            facility.IDPicture = "";

            foreach (var name in imageNames)
            {
                if (String.IsNullOrEmpty(facility.IDPicture))
                    facility.IDPicture = name;
                else
                    facility.IDPicture = facility.IDPicture + "," + name;
            }

            bool isUpdated = await ViewModel.ExecuteUpdateFacilityCommand(facility);
            MessageDialog messageDialog = new MessageDialog();
            if (isUpdated)
            {
               
                editButton.Visibility = ViewStates.Visible;
                saveButton.Visibility = ViewStates.Gone;
                messageDialog.SendToast("Facility is updated successful.");
            }
            else {
                messageDialog.SendToast("Facility is not updated successful.");
            }
            this.isEdit = false;
        }

        public string SaveImage(Bitmap bitmap)
        {
            string fileName;
            try
            {
                fileName = String.Format("facility_{0}", Guid.NewGuid());
                using (var os = new FileStream(_dir + "/" + fileName, FileMode.CreateNew))
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

        public void ShowSettingsAlert()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(Application.Context);
            Intent intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(intent);
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
                    tvfLatitude.Text = facility.Location.GPSCoordinates.Latitude;
                    tvfLongitude.Text = facility.Location.GPSCoordinates.Longitude;
                }

                if (facility.Location.BoundaryPolygon != null)
                {
                    foreach (var BoundaryPolygon in facility.Location.BoundaryPolygon.GPSCoordinates)
                    {
                        _BoundryPolygonGPSCoordinates.Add(BoundaryPolygon);
                        itemList.Add("Latitude: " + BoundaryPolygon.Latitude.ToString() + "      Longitude: " + BoundaryPolygon.Longitude.ToString());
                    }                   
                    arrayAdapter = new ArrayAdapter<string>(Activity, Resource.Layout.list_item, itemList);
                    bpListView.Adapter = arrayAdapter;
                    bpListView.ItemLongClick += Adapter_ItemSwipe;
                }
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
            else {
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
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
            GPSCoordinate thisGPSCoordinate = new GPSCoordinate()
            {
                Latitude = location.Latitude.ToString(),
                Longitude = location.Longitude.ToString()
            };
            _BoundryPolygonGPSCoordinates.Add(thisGPSCoordinate);
            itemList.Add("Latitude: " + location.Latitude.ToString() + "      Longitude: " + location.Longitude.ToString());

            if (location == null)
            {
                MessageDialog messageDialog = new MessageDialog();
                messageDialog.SendToast("Unable to get location");
            }

            arrayAdapter = new ArrayAdapter<string>(Activity, Resource.Layout.list_item, itemList);
            bpListView.Adapter = arrayAdapter;
            bpListView.ItemLongClick += Adapter_ItemSwipe;

        }
        void Adapter_ItemSwipe(object sender, ItemLongClickEventArgs e)
        {
            if (oldPosition != e.Position)
            {
                var item = arrayAdapter.GetItem(e.Position);
                arrayAdapter.Remove(item);
                //itemList.RemoveAt(e.Position);
                _BoundryPolygonGPSCoordinates.RemoveAt(e.Position);
                oldPosition = e.Position;
                bpListView.Adapter = arrayAdapter;
                bpListView.ItemLongClick += Adapter_ItemSwipe;
            }           
        }

        private void GPSLocationButton_Click(object sender, EventArgs eventArgs)
        {
            locationlinearlayout.Visibility = ViewStates.Visible;
            GPSTracker GPSTracker = new GPSTracker();

            Android.Locations.Location location = GPSTracker.GPSCoordinate();
            if (!GPSTracker.isLocationGPSEnabled)
            {
                ShowSettingsAlert();
            }
            tvfLatitude.Text = location.Latitude.ToString();
            tvfLongitude.Text = location.Longitude.ToString();

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
            if (!ValidateLocation())
                return;

            facility.Location = new Models.Location();
            facility.Location.LocalMunicipality = localmunicipality.SelectedItem.ToString();
            facility.Location.StreetAddress = streetAddress.Text;
            facility.Location.Suburb = suburb.Text;
            facility.Location.Region = region.Text;
            facility.Location.GPSCoordinates = new Models.GPSCoordinate() {
                Longitude = tvfLatitude.Text,
                Latitude = tvfLongitude.Text,
            };
            facility.Location.BoundaryPolygon = new Models.BoundryPolygon()
            {
                GPSCoordinates = _BoundryPolygonGPSCoordinates
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