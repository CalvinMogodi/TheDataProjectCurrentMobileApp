using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TheDataProject
{
    public class FacilitiesViewModel : BaseViewModel
    {
        public ObservableCollection<Facility> Facilities { get; set; }
        public Command LoadFacilitiesCommand { get; set; }
        public Command UpdateFacilityCommand { get; set; }

        public FacilitiesViewModel()
        {
            Title = "Facility";
            Facilities = new ObservableCollection<Facility>();
            LoadFacilitiesCommand = new Command<int>(async (int userId) => await ExecuteFacilitiesCommand(userId));
            UpdateFacilityCommand = new Command<Facility>(async (Facility facility) => await ExecuteUpdateFacilityCommand(facility));
        }
        
        public async Task ExecuteFacilitiesCommand(int userId)
        {
            ObservableCollection<Facility> facilities = new ObservableCollection<Facility>();

            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Facilities.Clear();
                Facilities = await DataStore.GetFacilitysAsync(userId);               
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
            //return Facilities;
        }

        public async Task<bool> ExecuteUpdateFacilityCommand(Facility facility)
        {
            return await DataStore.UpdateFacilityAsync(facility);
        }
    }
}
