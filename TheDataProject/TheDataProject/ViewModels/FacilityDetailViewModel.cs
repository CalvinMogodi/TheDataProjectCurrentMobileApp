using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TheDataProject
{
    public class FacilityDetailViewModel : BaseViewModel
    {
        public Facility Facility { get; set; }
        public Command GetFacilityDetailsCommand { get; set; }
        public FacilityDetailViewModel(Facility facility = null)
        {
            if (facility != null)
            {
                Title = facility.Name;
                //Facility = facility;

                GetFacilityDetailsCommand = new Command<Facility>(async (Facility thisFacility) => await ExecuteGetFacilityDetailsCommand(thisFacility.Id));
            }
        }

        async Task ExecuteGetFacilityDetailsCommand(int id)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Facility facility = await DataStore.GetFacilityAsync(id);
                Facility = facility;
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
