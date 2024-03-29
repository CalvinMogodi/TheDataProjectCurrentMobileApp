﻿using Microsoft.VisualBasic;
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
    public class MockDataStore : IDataStore<Facility, Building, User, Picture, DBPicture, Location, Person, DeedsInfo>
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
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Facility/UpdateFacility";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isUpdated = false;
            facility.Location = new Models.Location();
            facility.ResposiblePerson = new Person();
            facility.DeedsInfo = new DeedsInfo();
            var json = JsonConvert.SerializeObject(facility);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                var _content = await response.Content.ReadAsStringAsync();
                isUpdated = true;
            }

            return await Task.FromResult(isUpdated);
        }

        public async Task<Facility> GetFacilityAsync(int id)
        {
            return await Task.FromResult(facilities.FirstOrDefault(s => s.Id == id));
        }

        public async Task<ObservableCollection<Facility>> GetFacilitysAsync(int userId)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Facility/GetFacilitiesByUserId?userId=" + userId;
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
            } catch (Exception)
            {
                facilities = new ObservableCollection<Facility>();
                return await Task.FromResult(facilities);
            }
            return await Task.FromResult(facilities);
        }

        public async Task<bool> AddBuildingAsync(Building building)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Building/AddBuilding";
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
            catch (Exception)
            {
                return isAdded;
            }
            return await Task.FromResult(isAdded);
        }

        public async Task<bool> UpdateBuildingAsync(Building building)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Building/UpdateBuilding";
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
            catch (Exception)
            {
                return isUpdated;
            }
            return await Task.FromResult(isUpdated);
        }

        public async Task<ObservableCollection<Building>> GetBuildingsAsync(int facilityId)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Building/GetBuildingByFacilityId?facilityId=" + facilityId;
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
            } catch (Exception){
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
            string RestUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/User/Login?username=" + user.Username+"&password="+ user.Password;
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
             string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Building/SaveImage";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isSaved= false;
            try
            {
                var json = JsonConvert.SerializeObject(pictures);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //client.MaxResponseContentBufferSize = 256000;
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isSaved = true;
                }
            }
            catch (Exception ex)
            {
                return isSaved;
            }
            return await Task.FromResult(isSaved);
        }

        public async Task<Picture> GetImage(string fileName)
        {
            string RestUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Building/GetImage?pictureGuid=" + fileName;
            var uri = new Uri(string.Format(RestUrl, string.Empty));

            HttpResponseMessage response = null;
            Picture _picture = new Picture();
            try
            {
                response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var _content = await response.Content.ReadAsStringAsync();
                    var str = JsonConvert.DeserializeObject<string>(_content);
                    if (str != null)
                    {
                        _picture = new Picture() {
                            Name = fileName,
                            File = str,
                        };
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
            catch (Exception)
            {
                return _picture;
            }

            return _picture;
        }

        public async Task<bool> AddUpdateDeedsInfoAsync(DeedsInfo deedsInfo)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/DeedsInfo/CreateEdit";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isSuccess = false;
            try
            {
                var json = JsonConvert.SerializeObject(deedsInfo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
            }
            catch (Exception)
            {
                return isSuccess;
            }
            return await Task.FromResult(isSuccess);
        }

        public async Task<bool> AddUpdateLocationAsync(Location location)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Location/CreateEdit";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isSuccess = false;
            try
            {
                var json = JsonConvert.SerializeObject(location);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
            }
            catch (Exception)
            {
                return isSuccess;
            }
            return await Task.FromResult(isSuccess);
        }

        public async Task<bool> AddUpdatePersonAsync(Person person)
        {
            string restUrl = "https://amethysthemisphere.dedicated.co.za:81/theproject/api/Person/CreateEdit";
            var uri = new Uri(string.Format(restUrl, string.Empty));
            bool isSuccess = false;
            try
            {
                var json = JsonConvert.SerializeObject(person);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
            }
            catch (Exception)
            {
                return isSuccess;
            }
            return await Task.FromResult(isSuccess);
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
