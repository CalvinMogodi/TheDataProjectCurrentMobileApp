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

            var _facilities = new List<Facility>
            {
                new Facility { Id = Guid.NewGuid().ToString(), Name = "First item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Name = "Second item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Name = "Third item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Name = "Fourth item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Name = "Fifth item", Description="This is a nice description"},
                new Facility { Id = Guid.NewGuid().ToString(), Name = "Sixth item", Description="This is a nice description"},
            };

            var _buildings = new List<Building>
            {
                new Building { Id = Guid.NewGuid().ToString(), Name = "First item", Description="This is a nice description"},
                new Building { Id = Guid.NewGuid().ToString(), Name = "Second item", Description="This is a nice description"},
                new Building { Id = Guid.NewGuid().ToString(), Name = "Third item", Description="This is a nice description"},
                new Building { Id = Guid.NewGuid().ToString(), Name = "Fourth item", Description="This is a nice description"},
                new Building { Id = Guid.NewGuid().ToString(), Name = "Fifth item", Description="This is a nice description"},
                new Building { Id = Guid.NewGuid().ToString(), Name = "Sixth item", Description="This is a nice description"},
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

        public async Task<Facility> GetFacilityAsync(string id)
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

        public async Task<Building> GetBuildingAsync(string id)
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
