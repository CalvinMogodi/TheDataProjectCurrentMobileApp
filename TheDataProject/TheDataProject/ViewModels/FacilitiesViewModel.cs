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
            LoadFacilitiesCommand = new Command(async () => await ExecuteFacilitiesCommand());
            UpdateFacilityCommand = new Command<Facility>(async (Facility facility) => await ExecuteUpdateFacilityCommand(facility));
        }

        async Task ExecuteFacilitiesCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Facilities.Clear();
                var facilities = await DataStore.GetFacilitysAsync(true);
                foreach (var facility in facilities)
                {
                    Facilities.Add(facility);
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

        async Task ExecuteUpdateFacilityCommand(Facility facility)
        {
            await DataStore.UpdateFacilityAsync(facility);
        }
    }
}
