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
using System.Collections.ObjectModel;

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

        public async Task<bool> SyncFacilitiesFromAPI(ObservableCollection<Facility> facilities)
        {
            bool isSycned = false;
            var db = new SQLiteAsyncConnection(dbPath);
            ObservableCollection<LocalFacility> facilitiesToAddOnLocal = new ObservableCollection<LocalFacility>();
            ObservableCollection<LocalFacility> facilitiesToUpdateOnLocal = new ObservableCollection<LocalFacility>();
            ObservableCollection<Facility> facilitiesToUpdateOnAPI = new ObservableCollection<Facility>();
            foreach (var facility in facilities)
            {
                var _facility = await db.Table<LocalFacility>().Where(f => f.Id == facility.Id).FirstOrDefaultAsync();
                if (_facility != null)
                {
                    if (_facility.ModifiedDate > facility.ModifiedDate)
                    {
                        facilitiesToUpdateOnAPI.Add(MapLocalLocalFacilityToFacility(_facility));
                    }
                    else if (_facility.ModifiedDate < facility.ModifiedDate) {
                        facilitiesToUpdateOnLocal.Add(MapFacilityToLocalFacility(facility));
                    }
                }
                else {
                    facilitiesToAddOnLocal.Add(_facility);
                }
            }

            if (facilitiesToAddOnLocal.Count > 0)
            {
               await InsertFacilities(facilitiesToAddOnLocal);
            }
            if (facilitiesToUpdateOnLocal.Count > 0)
            {
                await InsertFacilities(facilitiesToAddOnLocal);
            }
            return await Task.FromResult(isSycned);
        }

        public async Task<bool> SyncBuildingsFromAPI(ObservableCollection<Models.Building> buildings)
        {
            bool isSycned = false;
            var db = new SQLiteAsyncConnection(dbPath);
            ObservableCollection<LocalBuilding> buildingsToAddOnLocal = new ObservableCollection<LocalBuilding>();
            ObservableCollection<LocalBuilding> buildingsToUpdateOnLocal = new ObservableCollection<LocalBuilding>();
            ObservableCollection<Models.Building> buildingsToUpdateOnAPI = new ObservableCollection<Models.Building>();
            foreach (var building in buildings)
            {
                var _building = await db.Table<LocalBuilding>().Where(f => f.Id == building.Id).FirstOrDefaultAsync();
                if (_building != null)
                {
                    if (_building.ModifiedDate > building.ModifiedDate)
                    {
                        buildingsToUpdateOnAPI.Add(MapLocalBuildingToBuilding(_building));
                    }
                    else if (_building.ModifiedDate < building.ModifiedDate)
                    {
                        buildingsToUpdateOnLocal.Add(MapBuildingToLocalBuilding(building));
                    }
                }
                else
                {
                    buildingsToAddOnLocal.Add(_building);
                }
            }
            return await Task.FromResult(isSycned);
        }
        
        #region User
        public async Task<string> InsertUpdateUser(LocalUser user)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                var thisUser = await db.Table<LocalUser>().OrderByDescending(t => t.Username).FirstOrDefaultAsync();
                if (thisUser != null)
                {
                    //if (thisUser.Id != user.Id)
                    //    await db.UpdateAsync(user);
                }
                else {
                    await db.InsertAsync(user);
                }
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
        #endregion #User

        #region Facility
        public async Task<bool> InsertFacilities(ObservableCollection<LocalFacility> facilities)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                await db.InsertAllAsync(facilities);
                return true;
            }
            catch (SQLiteException ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateFacility(LocalFacility facility)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                await db.UpdateAsync(facility);
                return true;
            }
            catch (SQLiteException ex)
            {
                return false;
            }
        }

        public async Task<ObservableCollection<LocalFacility>> GetFacilities(int userId)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                return new ObservableCollection<LocalFacility>(await db.Table<LocalFacility>().Where(u => u.CreatedUserId == userId).ToListAsync());
            }
            catch (SQLiteException ex)
            {
                return new ObservableCollection<LocalFacility>();
            }
        }

        public LocalFacility MapFacilityToLocalFacility(Facility facility)
        {
            LocalFacility LocalFacility = new LocalFacility()
            {
                Id = facility.Id,
                Name = facility.Name,
                ClientCode = facility.ClientCode,
                SettlementType = facility.SettlementType,
                Zoning = facility.Zoning,
                IDPicture = facility.IDPicture,
                Status = facility.Status,
                CreatedDate = facility.CreatedDate,
                CreatedUserId = facility.CreatedUserId,
                ModifiedDate = facility.ModifiedDate,
                ModifiedUserId = facility.ModifiedUserId,
            };
            if (facility.DeedsInfo != null)
            {
                LocalFacility.LocalDeedsInfo = new LocalDeedsInfo()
                {
                    Id = facility.DeedsInfo.Id,
                    ErFNumber = facility.DeedsInfo.ErFNumber,
                    TitleDeedNumber = facility.DeedsInfo.TitleDeedNumber,
                    Extent = facility.DeedsInfo.Extent,
                    OwnerInfomation = facility.DeedsInfo.OwnerInfomation,
                };
            }
            if (facility.ResposiblePerson != null)
            {
                LocalFacility.LocalResposiblePerson = new LocalPerson()
                {
                    Id = facility.ResposiblePerson.Id,
                    FullName = facility.ResposiblePerson.FullName,
                    PhoneNumber = facility.ResposiblePerson.PhoneNumber,
                    Designation = facility.ResposiblePerson.Designation,
                    EmailAddress = facility.ResposiblePerson.EmailAddress,
                };
            }
            if (facility.Location != null)
            {
                LocalLocation LocalLocation = new LocalLocation()
                {
                    Id = facility.Location.Id,
                    LocalMunicipality = facility.Location.Id,
                    StreetAddress = facility.Location.StreetAddress,
                    Suburb = facility.Location.Suburb,
                    Province = facility.Location.Province,
                    Region = facility.Location.Region,
                };
                if (facility.Location.GPSCoordinates != null)
                {
                    LocalGPSCoordinate LocalGPSCoordinates = new LocalGPSCoordinate()
                    {
                        Id = facility.Location.GPSCoordinates.Id,
                        Longitude = facility.Location.GPSCoordinates.Longitude,
                        Latitude = facility.Location.GPSCoordinates.Latitude,
                    };
                }
                if (facility.Location.BoundryPolygon != null)
                {
                    List<LocalBoundryPolygon> LocalBoundryPolygon = new List<LocalBoundryPolygon>();
                    foreach (var item in facility.Location.BoundryPolygon)
                    {
                        LocalBoundryPolygon.Add(new LocalBoundryPolygon()
                        {
                            Longitude = item.Longitude,
                            Latitude = item.Latitude
                        });
                    }
                }
            }
            return LocalFacility;
        }

        public Facility MapLocalLocalFacilityToFacility(LocalFacility facility)
        {
            Facility Facility = new Facility()
            {
                Id = facility.Id,
                Name = facility.Name,
                ClientCode = facility.ClientCode,
                SettlementType = facility.SettlementType,
                Zoning = facility.Zoning,
                IDPicture = facility.IDPicture,
                Status = facility.Status,
                CreatedDate = facility.CreatedDate,
                CreatedUserId = facility.CreatedUserId,
                ModifiedDate = facility.ModifiedDate,
                ModifiedUserId = facility.ModifiedUserId,
            };
            if (facility.LocalDeedsInfo != null)
            {
                Facility.DeedsInfo = new Models.DeedsInfo()
                {
                    Id = facility.LocalDeedsInfo.Id,
                    ErFNumber = facility.LocalDeedsInfo.ErFNumber,
                    TitleDeedNumber = facility.LocalDeedsInfo.TitleDeedNumber,
                    Extent = facility.LocalDeedsInfo.Extent,
                    OwnerInfomation = facility.LocalDeedsInfo.OwnerInfomation,
                };
            }
            if (facility.LocalResposiblePerson != null)
            {
                Facility.ResposiblePerson = new Models.Person()
                {
                    Id = facility.LocalResposiblePerson.Id,
                    FullName = facility.LocalResposiblePerson.FullName,
                    PhoneNumber = facility.LocalResposiblePerson.PhoneNumber,
                    Designation = facility.LocalResposiblePerson.Designation,
                    EmailAddress = facility.LocalResposiblePerson.EmailAddress,
                };
            }
            if (facility.LocalLocation != null)
            {
                Models.Location LocalLocation = new Models.Location()
                {
                    Id = facility.LocalLocation.Id,
                    LocalMunicipality = facility.LocalLocation.Id,
                    StreetAddress = facility.LocalLocation.StreetAddress,
                    Suburb = facility.LocalLocation.Suburb,
                    Province = facility.LocalLocation.Province,
                    Region = facility.LocalLocation.Region,
                };
                if (facility.LocalLocation.LocalGPSCoordinates != null)
                {
                    Models.GPSCoordinate LocalGPSCoordinates = new Models.GPSCoordinate()
                    {
                        Id = facility.LocalLocation.LocalGPSCoordinates.Id,
                        Longitude = facility.LocalLocation.LocalGPSCoordinates.Longitude,
                        Latitude = facility.LocalLocation.LocalGPSCoordinates.Latitude,
                    };
                }
                if (facility.LocalLocation.LocalBoundryPolygon != null)
                {
                    List<Models.BoundryPolygon> LocalBoundryPolygon = new List<Models.BoundryPolygon>();
                    foreach (var item in facility.LocalLocation.LocalBoundryPolygon)
                    {
                        LocalBoundryPolygon.Add(new Models.BoundryPolygon()
                        {
                            Longitude = item.Longitude,
                            Latitude = item.Latitude
                        });
                    }
                }
            }
            return Facility;
        }
        #endregion #Facility

        #region Building

        public async Task<bool> InsertBuilding(LocalBuilding building)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                await db.InsertAsync(building);
                return true;
            }
            catch (SQLiteException ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUser(LocalBuilding building)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                await db.UpdateAsync(building);
                return true;
            }
            catch (SQLiteException ex)
            {
                return false;
            }
        }
        public async Task<ObservableCollection<LocalBuilding>> GetUser(int facilityId)
        {
            try
            {
                var db = new SQLiteAsyncConnection(dbPath);
                return new ObservableCollection<LocalBuilding> (await db.Table<LocalBuilding>().Where(f => f.LocalFacility.Id == facilityId).ToListAsync());
            }
            catch (SQLiteException ex)
            {
                return new ObservableCollection<LocalBuilding>();
            }
        }

        public LocalBuilding MapBuildingToLocalBuilding(Models.Building building)
        {
            LocalBuilding LocalBuilding =  new LocalBuilding()
            {
                Id = building.Id,
                BuildingNumber = building.BuildingNumber,
                BuildingName = building.BuildingName,
                BuildingType = building.BuildingType,
                BuildingStandard = building.BuildingStandard,
                Status = building.Status,
                NumberOfFloors = building.NumberOfFloors,
                FootPrintArea = building.FootPrintArea,
                ImprovedArea = building.ImprovedArea,
                Heritage = building.Heritage,
                OccupationYear = building.OccupationYear,
                DisabledAccess = building.DisabledAccess,
                DisabledComment = building.DisabledComment,
                ConstructionDescription = building.ConstructionDescription,
                Photo = building.Photo,
                CreatedDate = building.CreatedDate,
                CreatedUserId = building.CreatedUserId,
                ModifiedDate = building.ModifiedDate,
                ModifiedUserId = building.ModifiedUserId,
            };
            if (building.Facility != null) {
                LocalBuilding.LocalFacility = new LocalFacility() {
                    Id = building.Facility.Id
                };
            }

            if (building.GPSCoordinates != null)
            {
                LocalBuilding.LocalGPSCoordinate = new LocalGPSCoordinate()
                {
                    Id = building.GPSCoordinates.Id,
                    Longitude = building.GPSCoordinates.Longitude,
                    Latitude = building.GPSCoordinates.Latitude,
                };
            }
            return LocalBuilding;
        }

        public Models.Building MapLocalBuildingToBuilding(LocalBuilding building)
        {
            Models.Building Building = new Models.Building()
            {
                Id = building.Id,
                BuildingNumber = building.BuildingNumber,
                BuildingName = building.BuildingName,
                BuildingType = building.BuildingType,
                BuildingStandard = building.BuildingStandard,
                Status = building.Status,
                NumberOfFloors = building.NumberOfFloors,
                FootPrintArea = building.FootPrintArea,
                ImprovedArea = building.ImprovedArea,
                Heritage = building.Heritage,
                OccupationYear = building.OccupationYear,
                DisabledAccess = building.DisabledAccess,
                DisabledComment = building.DisabledComment,
                ConstructionDescription = building.ConstructionDescription,
                Photo = building.Photo,
                CreatedDate = building.CreatedDate,
                CreatedUserId = building.CreatedUserId,
                ModifiedDate = building.ModifiedDate,
                ModifiedUserId = building.ModifiedUserId,
            };
            if (building.LocalFacility != null)
            {
                Building.Facility = new Facility()
                {
                    Id = building.LocalFacility.Id
                };
            }

            if (building.LocalGPSCoordinate != null)
            {
                Building.GPSCoordinates = new Models.GPSCoordinate()
                {
                    Id = building.LocalGPSCoordinate.Id,
                    Longitude = building.LocalGPSCoordinate.Longitude,
                    Latitude = building.LocalGPSCoordinate.Latitude,
                };
            }
            return Building;
        }
        #endregion #Building
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
        public LocalFacility LocalFacility { get; set; }
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