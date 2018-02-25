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
using SQLite;
using System.Threading.Tasks;
using System.IO;

namespace TheDataProject.Droid.Helpers
{
    public class SqlLiteManager
    {
        public static String dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "DataProjectDB.db3");
        public string CreateDatabase()
        {
            try
            {
                var connection = new SQLiteAsyncConnection(dbPath);
                connection.CreateTableAsync<LocalUser>();
                connection.CreateTableAsync<LocalFacility>();
                connection.CreateTableAsync<LocalBuilding>();
                connection.CreateTableAsync<LocalBoundryPolygon>();
                connection.CreateTableAsync<LocalDeedsInfo>();
                connection.CreateTableAsync<LocalGPSCoordinate>();
                connection.CreateTableAsync<LocalLocation>();
                connection.CreateTableAsync<LocalPerson>();
                return "Database created";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> InsertUpdateUser(LocalUser user)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                var thisUser = await db.Table<LocalUser>().OrderByDescending(t => t.Username).FirstOrDefaultAsync();
                if (thisUser != null)
                   await db.UpdateAsync(user);
                else
                    await db.InsertAsync(user);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public async Task<LocalUser> GetUser(LocalUser user)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                var thisUser = await db.Table<LocalUser>().Where(u => u.Username == user.Username).FirstOrDefaultAsync();

                return thisUser;
            }
            catch (SQLiteException ex)
            {
                return null;
            }
        }

        public LocalUser MapUserToLocalUser(Models.User user) {
           return new LocalUser()
            {
                Id = user.Id,
                Username = user.Username,
               Password = user.Password,
            };
        }

        public Models.User MapLocalUserToUser(LocalUser user)
        {
            return new Models.User()
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
            };
        }
    }

    #region Local Models
    public class LocalUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }      
    }

    public class LocalFacility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientCode { get; set; }
        public string SettlementType { get; set; }
        public string Zoning { get; set; }
        public string IDPicture { get; set; }
        public LocalDeedsInfo LocalDeedsInfo { get; set; }
        public LocalPerson LocalResposiblePerson { get; set; }
        public LocalLocation LocalLocation { get; set; }
        public List<LocalBuilding> Buildings { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ModifiedUserId { get; set; }
    }

    public class LocalBoundryPolygon
    {
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public class LocalBuilding
    {
        public int Id { get; set; }
        public string BuildingNumber { get; set; }
        public string BuildingName { get; set; }
        public string BuildingType { get; set; }
        public string BuildingStandard { get; set; }
        public string Status { get; set; }
        public int NumberOfFloors { get; set; }
        public double FootPrintArea { get; set; }
        public double ImprovedArea { get; set; }
        public bool Heritage { get; set; }
        public string OccupationYear { get; set; }
        public string DisabledAccess { get; set; }
        public string DisabledComment { get; set; }
        public string ConstructionDescription { get; set; }
        public LocalGPSCoordinate LocalGPSCoordinate { get; set; }
        public string Photo { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? ModifiedUserId { get; set; }
        public Facility Facility { get; set; }
    }

    public class LocalDeedsInfo
    {
        public int Id { get; set; }
        public string ErFNumber { get; set; }
        public string TitleDeedNumber { get; set; }
        public string Extent { get; set; }
        public string OwnerInfomation { get; set; }
    }

    public class LocalGPSCoordinate
    {
        public int Id { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public class LocalLocation
    {
        public string Id { get; set; }
        public string LocalMunicipality { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Province { get; set; }
        public string Region { get; set; }
        public LocalGPSCoordinate LocalGPSCoordinates { get; set; }
        public List<LocalBoundryPolygon> LocalBoundryPolygon { get; set; }
    }

    public class LocalPerson
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Designation { get; set; }
        public string EmailAddress { get; set; }
    }

    #endregion #endLocal Models
}