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
    public class MockDataStore : IDataStore<Facility, Building, User, Picture>
    {

        HttpClient client;
        ObservableCollection<Facility> facilities;
        ObservableCollection<Building> buildings;

        public MockDataStore()
        {
            
            client = new HttpClient();
           // client.MaxResponseContentBufferSize = 256000;            
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
            try
            {
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    facilities = JsonConvert.DeserializeObject<ObservableCollection<Facility>>(content);
                }
                else if (response == null) {
                    facilities = new ObservableCollection<Facility>();
                }
            } catch (Exception ex)
            {
                facilities = new ObservableCollection<Facility>();
                return await Task.FromResult(facilities);
            }
            return await Task.FromResult(facilities);
        }

        public async Task<bool> AddBuildingAsync(Building building)
        {
            string restUrl = "http://154.0.170.81:89/api/Building/AddBuilding";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isAdded = false;
            try
            {
                var json = JsonConvert.SerializeObject(building);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isAdded = true;
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
            string restUrl = "http://154.0.170.81:89/api/Building/UpdateBuilding";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isUpdated = false;
            try
            {
                var json = JsonConvert.SerializeObject(building);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isUpdated = true;
                }
            }
            catch (Exception ex)
            {
                return isUpdated;
            }
            return await Task.FromResult(isUpdated);
        }

        public async Task<ObservableCollection<Building>> GetBuildingsAsync(int facilityId)
        {
            string restUrl = "http://154.0.170.81:89/api/Building/GetBuildingByFacilityId?facilityId=" + facilityId;
            var uri = new Uri(string.Format(restUrl, string.Empty));
            try { 
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    buildings = JsonConvert.DeserializeObject<ObservableCollection<Building>>(content);
                    if (buildings == null)
                    {
                        buildings = new ObservableCollection<Building>();
                    }
                }
            } catch (Exception ex){
                buildings = new ObservableCollection<Building>();
                return await Task.FromResult(buildings);
            }
            if (buildings == null)
            {
                buildings = new ObservableCollection<Building>();
            }
            return await Task.FromResult(buildings);
        }

        public async Task<User> LoginUser(User user)
        {
            string RestUrl = "http://154.0.170.81:89/api/User/Login?username="+user.Username+"&password="+ user.Password;
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            HttpResponseMessage response = null;
            User _user = new User();
            try
            {
                response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    _user = JsonConvert.DeserializeObject<User>(_content);
                    if (_user == null)
                    {
                        _user = new User()
                        {
                            RespondMessage = "Invaild username or password.",
                        };
                    }
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

        public async Task<bool> SaveImage(List<Picture> pictures)
        {
             string restUrl = "http://154.0.170.81:89/api/Building/SaveImage";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isSaved= false;
            try
            {
                var json = JsonConvert.SerializeObject(pictures);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.MaxResponseContentBufferSize = 256000;
                var response = await client.PostAsync(uri, content);
                //if (response.IsSuccessStatusCode)
                //{
                //    isSaved = true;
                //}
            }
            catch (Exception ex)
            {
                return isSaved;
            }
            return await Task.FromResult(isSaved);
        }

        public async Task<Picture> GetImage(string fileName)
        {
            string RestUrl = "http://154.0.170.81:89/api/Building/GetImage?pictureGuid=" + fileName;
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            HttpResponseMessage response = null;
            Picture _picture = new Picture();
            try
            {
                response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    _picture = JsonConvert.DeserializeObject<Picture>(_content);
                    if (_picture == null)
                    {
                        _picture = new Picture();
                    }
                }

                else
                {
                    if (_picture == null)
                    {
                        _picture = new Picture();
                    }
                }
            }
            catch (Exception ex)
            {
                return _picture;
            }

            return _picture;
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
