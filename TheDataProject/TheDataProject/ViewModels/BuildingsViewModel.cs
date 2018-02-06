using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class BuildingsViewModel : BaseViewModel
    {
        public ObservableCollection<Building> Buildings { get; set; }
        public Command AddBuildingCommand { get; set; }

        public BuildingsViewModel()
        {
            Title = "Building";
            Buildings = new ObservableCollection<Building>();
            AddBuildingCommand = new Command<Building>(async (Building building) => await AddBuildingAsync(building));
        }

        public async Task ExecuteBuildingsCommand(int facilityId)
        {
            Buildings.Clear();
            Buildings = await DataStore.GetBuildingsAsync(facilityId);
        }

        public async Task<bool> AddBuildingAsync(Building building)
        {
            Buildings.Add(building);
            return await DataStore.AddBuildingAsync(building);           
        }

        public async Task<bool> UpdateBuildingAsync(Building building)
        {
            return await DataStore.UpdateBuildingAsync(building);
        }
    }
}
