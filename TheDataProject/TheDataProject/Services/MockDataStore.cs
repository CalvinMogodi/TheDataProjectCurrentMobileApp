using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject
{
    public class MockDataStore : IDataStore<Facility, Building, User>
    {

        HttpClient client;
        ObservableCollection<Facility> facilities;
        List<Building> buildings;

        public MockDataStore()
        {
            
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;            
        }

        public async Task<bool> UpdateFacilityAsync(Facility facility)
        {
            string restUrl = "http://154.0.170.81:89/api/Facility/UpdateFacility";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isUpdated = false;

            var json = JsonConvert.SerializeObject(facility);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                var _content = await response.Content.ReadAsStringAsync();
                isUpdated = JsonConvert.DeserializeObject<bool>(_content);
            }

            return await Task.FromResult(isUpdated);
        }

        public async Task<Facility> GetFacilityAsync(int id)
        {
            return await Task.FromResult(facilities.FirstOrDefault(s => s.Id == id));
        }

        public async Task<ObservableCollection<Facility>> GetFacilitysAsync(int userId)
        {
            string restUrl = "http://154.0.170.81:89/api/Facility/GetFacilitiesByUserId?userId=" + userId;
            var uri = new Uri(string.Format(restUrl, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                facilities = JsonConvert.DeserializeObject<ObservableCollection<Facility>>(content);
            }

            return await Task.FromResult(facilities);
        }

        public async Task<bool> AddBuildingAsync(Building building)
        {
            string restUrl = "http://154.0.170.81:89/api/Building/AddBulding";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isAdded = false;
            try
            {
                var json = JsonConvert.SerializeObject(building);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    isAdded = JsonConvert.DeserializeObject<bool>(_content);
                }
            }
            catch (Exception ex)
            {
                return isAdded;
            }
            return await Task.FromResult(isAdded);
        }

        public async Task<bool> UpdateBuildingAsync(Building building)
        {
            string restUrl = "http://154.0.170.81:89/api/Building/UpdateFacility";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isUpdated = false;
            try
            {
                var json = JsonConvert.SerializeObject(building);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    isUpdated = JsonConvert.DeserializeObject<bool>(_content);
                }
            }
            catch (Exception ex)
            {
                return isUpdated;
            }
            return await Task.FromResult(isUpdated);
        }

        public async Task<ObservableCollection<Building>> GetBuildingsAsync(bool forceRefresh = false)
        {
            return null;// await Task.FromResult(buildings);
        }

        public async Task<User> LoginUser(User user)
        {
            string RestUrl = "http://154.0.170.81:89/api/User/Login";
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            HttpResponseMessage response = null;
            User _user = new User();
            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await client.PutAsync(json, content);

                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    _user = JsonConvert.DeserializeObject<User>(_content);
                }
                else if(_user == user) {
                    _user.RespondMessage = "Invaild username or password.";
                }
                else
                {
                    _user.RespondMessage = "Error occurred: Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _user.RespondMessage = "Error occurred: Please try again later.";              
                return _user;
            }
            
            return _user;
        }

        public Task<User> ChangePassword(User user)
        {
            return Task.FromResult(user);
        }
    }

    public class RestService 
    {
        HttpClient client;
        public RestService()
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
        }
}

}
