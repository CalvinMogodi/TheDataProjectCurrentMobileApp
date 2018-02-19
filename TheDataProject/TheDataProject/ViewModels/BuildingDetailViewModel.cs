using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDataProject.Models;

namespace TheDataProject.ViewModels
{
    public class BuildingDetailViewModel : BaseViewModel
    {
        public Building Building { get; set; }
        public BuildingDetailViewModel(Building building = null)
        {
            if (building != null)
            {
                Title = building.BuildingName;
                Building = building;
            }
        }
    }
}
