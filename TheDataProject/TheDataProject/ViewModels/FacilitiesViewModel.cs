using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TheDataProject
{
    public class FacilitiesViewModel : BaseViewModel
    {
        public ObservableCollection<Facility> Facilities { get; set; }
        public ObservableCollection<Facility> OriginalFacilities { get; set; }
        public Command LoadFacilitiesCommand { get; set; }
        public Command UpdateFacilityCommand { get; set; }

        public FacilitiesViewModel()
        {
            Title = "Facility";
            Facilities = new ObservableCollection<Facility>();
            OriginalFacilities = new ObservableCollection<Facility>();
            LoadFacilitiesCommand = new Command<int>(async (int userId) => await ExecuteFacilitiesCommand(userId));
            UpdateFacilityCommand = new Command<Facility>(async (Facility facility) => await ExecuteUpdateFacilityCommand(facility));
        }
        
        public async Task ExecuteFacilitiesCommand(int userId)
        {
            Facilities.Clear();
            OriginalFacilities.Clear();
            Facilities = await DataStore.GetFacilitysAsync(userId);
            OriginalFacilities = Facilities;
        }

        public async Task<bool> ExecuteUpdateFacilityCommand(Facility facility)
        {
            return await DataStore.UpdateFacilityAsync(facility);
        }
    }
}
