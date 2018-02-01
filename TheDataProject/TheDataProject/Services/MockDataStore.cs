using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject
{
    public class MockDataStore : IDataStore<Facility, Building, User>
    {
        List<Facility> facilities;
        List<Building> buildings;

        public MockDataStore()
        {
            facilities = new List<Facility>();
            buildings = new List<Building>();
            GPSCoordinate coordinate = new GPSCoordinate() { };
            BoundryPolygon boundryPolygon = new BoundryPolygon() { };
            DeedsInfo deedsInfo = new DeedsInfo() { };
            Person resposiblePerson = new Person();
            Location location = new Location() { };
            List<Building> buildingList = new List<Building>() { };

            var _facilities = new List<Facility>
            {
                new Facility { Id = 1, Name = "First item", ClientCode="3225664000001",SettlementType="Formal - formal",Zoning="2",
                    MunicipalRoll = "", IDPicture = "", GPSCoordinates = coordinate, Polygon = boundryPolygon,DeedsInfo = deedsInfo,
                    ResposiblePerson = resposiblePerson,Location = location, Buildings = buildingList, CreatedDate = new DateTime(),
                    CreatedUserId = 1 ,ModifiedDate = new DateTime(), ModifiedUserId =1    },
                new Facility { Id = 2, Name = "Second item", ClientCode="This is a nice description"},
                new Facility { Id = 3, Name = "Third item", ClientCode="This is a nice description"},
                new Facility { Id = 4, Name = "Fourth item", ClientCode="This is a nice description"},
                new Facility { Id = 5, Name = "Fifth item", ClientCode="This is a nice description"},
                new Facility { Id = 6, Name = "Sixth item", ClientCode="This is a nice description"},
            };

            var _buildings = new List<Building>
            {
                new Building { Id = 1, Name = "First item", BuildingNumber="This is a nice description"},
                new Building { Id = 2, Name = "Second item", BuildingNumber="This is a nice description"},
                new Building { Id = 3, Name = "Third item", BuildingNumber="This is a nice description"},
                new Building { Id = 4, Name = "Fourth item", BuildingNumber="This is a nice description"},
                new Building { Id = 5, Name = "Fifth item", BuildingNumber="This is a nice description"},
                new Building { Id = 6, Name = "Sixth item", BuildingNumber="This is a nice description"},
            };

            foreach (Facility facility in _facilities)
            {
                facilities.Add(facility);
            }

            foreach (Building building in _buildings)
            {
                buildings.Add(building);
            }
        }

        public async Task<bool> UpdateFacilityAsync(Facility facility)
        {
            var _facility = facilities.Where((Facility arg) => arg.Id == facility.Id).FirstOrDefault();
            facilities.Remove(_facility);
            facilities.Add(facility);

            return await Task.FromResult(true);
        }

        public async Task<Facility> GetFacilityAsync(int id)
        {
            return await Task.FromResult(facilities.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Facility>> GetFacilitysAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(facilities);
        }

        public Task<bool> AddBuildingAsync(Building building)
        {
            buildings.Add(building);
            return Task.FromResult(true);
        }

        public async Task<Building> GetBuildingAsync(int id)
        {
            return await Task.FromResult(buildings.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Building>> GetBuildingsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(buildings);
        }

        public Task<bool> LoginUser(User user)
        {
            return Task.FromResult(true);
        }

        public Task<User> ChangePassword(User user)
        {
            return Task.FromResult(user);
        }
    }
}
