using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class PictureViewModel : BaseViewModel
    {
        public Command LoadFacilitiesCommand { get; set; }
        public Command UpdateFacilityCommand { get; set; }

        public PictureViewModel()
        {
            LoadFacilitiesCommand = new Command<string>(async (string fileName) => await ExecuteGetPictureCommand(fileName));
            UpdateFacilityCommand = new Command<List<Picture>>(async (List<Picture> pictures) => await ExecuteSavePictureCommand(pictures));
        }

        public async Task<Picture> ExecuteGetPictureCommand(string fileName)
        {
            return await DataStore.GetImage(fileName);
        }

        public async Task<bool> ExecuteSavePictureCommand(List<Picture> pictures)
        {
            return await DataStore.SaveImage(pictures);
        }
    }
}
