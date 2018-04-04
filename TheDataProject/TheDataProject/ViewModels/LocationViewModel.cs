using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class LocationViewModel : BaseViewModel
    {
        public Location Location { get; set; }
        public LocationViewModel(Location location = null)
        {
            if (location != null)
            {
                Location = location;
            }
        }

        public async Task<bool> AddUpdateLocationAsync(Location location)
        {
            return await DataStore.AddUpdateLocationAsync(location);
        }
    }
}
