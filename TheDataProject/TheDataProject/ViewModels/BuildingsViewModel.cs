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
        public Command LoadBuildingsCommand { get; set; }
        public Command AddBuildingCommand { get; set; }

        public BuildingsViewModel()
        {
            Title = "Building";
            Buildings = new ObservableCollection<Building>();
            LoadBuildingsCommand = new Command(async () => await ExecuteBuildingsCommand());
            AddBuildingCommand = new Command<Building>(async (Building building) => await AddBuildingAsync(building));
        }

        async Task ExecuteBuildingsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Buildings.Clear();
                var buildings = await DataStore.GetBuildingsAsync(true);
                foreach (var building in buildings)
                {
                    Buildings.Add(building);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task AddBuildingAsync(Building building)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Buildings.Add(building);
                await DataStore.AddBuildingAsync(building);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
